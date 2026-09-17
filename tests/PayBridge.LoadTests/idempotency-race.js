import http from 'k6/http';
import { check } from 'k6';
import { Counter, Trend } from 'k6/metrics';
import exec from 'k6/execution';

const serverErrors = new Counter('server_errors');
const businessFailures = new Counter('business_failures');
const conflictResponses = new Counter('conflict_responses');
const unexpectedResponses = new Counter('unexpected_responses');

const duration25 = new Trend('payment_duration_25_rps', true);
const duration50 = new Trend('payment_duration_50_rps', true);
const duration75 = new Trend('payment_duration_75_rps', true);
const duration100 = new Trend('payment_duration_100_rps', true);

export const options = {
    insecureSkipTLSVerify: true,

    scenarios: {
        load_25_rps: {
            executor: 'constant-arrival-rate',
            rate: 25,
            timeUnit: '1s',
            duration: '60s',

            preAllocatedVUs: 50,
            maxVUs: 150,

            startTime: '0s'
        },

        load_50_rps: {
            executor: 'constant-arrival-rate',
            rate: 50,
            timeUnit: '1s',
            duration: '60s',

            preAllocatedVUs: 100,
            maxVUs: 250,

            startTime: '65s'
        },

        load_75_rps: {
            executor: 'constant-arrival-rate',
            rate: 75,
            timeUnit: '1s',
            duration: '60s',

            preAllocatedVUs: 150,
            maxVUs: 350,

            startTime: '130s'
        },

        load_100_rps: {
            executor: 'constant-arrival-rate',
            rate: 100,
            timeUnit: '1s',
            duration: '60s',

            preAllocatedVUs: 200,
            maxVUs: 500,

            startTime: '195s'
        }
    },

    thresholds: {
        server_errors: [
            'count==0'
        ],

        unexpected_responses: [
            'count==0'
        ],

        conflict_responses: [
            'count==0'
        ]
    }
};

const BASE_URL = 'https://localhost:7166';

export function setup() {

    const tokenPayload = JSON.stringify({
        clientCode: 'mock-integration',
        clientSecret: 'mock-secret-123'
    });

    const tokenResponse = http.post(
        `${BASE_URL}/api/integration-tokens`,
        tokenPayload,
        {
            headers: {
                'Content-Type': 'application/json'
            }
        }
    );

    console.log(
        `Token status: ${tokenResponse.status}`
    );

    if (
        tokenResponse.status < 200 ||
        tokenResponse.status >= 300
    ) {
        throw new Error(
            `Token alınamadı. ` +
            `Status=${tokenResponse.status} | ` +
            `Body=${tokenResponse.body}`
        );
    }

    const body = tokenResponse.json();

    const token =
        body.accessToken ||
        body.token ||
        body.data?.accessToken ||
        body.data?.token;

    if (!token) {
        throw new Error(
            `Token response içinde token bulunamadı: ` +
            tokenResponse.body
        );
    }

    return {
        token,
        orderPrefix: `LOAD-SUSTAINED-${Date.now()}`
    };
}

export default function (data) {

    const scenarioName =
        exec.scenario.name;

    /*
        Her iteration benzersiz OrderId üretir.
        Böylece idempotency conflict beklemiyoruz.
    */
    const orderId =
        `${data.orderPrefix}-${scenarioName}-${__VU}-${__ITER}-${Date.now()}`;

    const paymentPayload = JSON.stringify({
        integrationClientId:
            '11111111-1111-1111-1111-111111111111',

        clientCode:
            'mock-integration',

        merchantCode:
            'MOCK-MERCHANT',

        orderId:
            orderId,

        amount:
            10,

        currency:
            'TRY',

        providerCode:
            'Mock',

        channel:
            'ECommerce'
    });

    const response = http.post(
        `${BASE_URL}/api/payments`,
        paymentPayload,
        {
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${data.token}`
            },

            tags: {
                test_type: 'sustained-payment-load',
                load_stage: scenarioName
            },

            responseCallback:
                http.expectedStatuses(200)
        }
    );

    /*
        Her yük seviyesinin latency'sini
        ayrı Trend metric'e yazıyoruz.
    */
    switch (scenarioName) {

        case 'load_25_rps':
            duration25.add(
                response.timings.duration
            );
            break;

        case 'load_50_rps':
            duration50.add(
                response.timings.duration
            );
            break;

        case 'load_75_rps':
            duration75.add(
                response.timings.duration
            );
            break;

        case 'load_100_rps':
            duration100.add(
                response.timings.duration
            );
            break;
    }

    switch (response.status) {

        case 200:
            break;

        case 400:

            businessFailures.add(1);

            console.error(
                `BUSINESS FAILURE | ` +
                `Scenario=${scenarioName} | ` +
                `VU=${__VU} | ` +
                `OrderId=${orderId} | ` +
                `Body=${response.body}`
            );

            break;

        case 409:

            conflictResponses.add(1);

            console.error(
                `UNEXPECTED CONFLICT | ` +
                `Scenario=${scenarioName} | ` +
                `VU=${__VU} | ` +
                `OrderId=${orderId} | ` +
                `Body=${response.body}`
            );

            break;

        default:

            if (response.status >= 500) {

                serverErrors.add(1);

                console.error(
                    `SERVER ERROR | ` +
                    `Scenario=${scenarioName} | ` +
                    `VU=${__VU} | ` +
                    `Status=${response.status} | ` +
                    `OrderId=${orderId} | ` +
                    `Body=${response.body}`
                );
            }
            else {

                unexpectedResponses.add(1);

                console.error(
                    `UNEXPECTED RESPONSE | ` +
                    `Scenario=${scenarioName} | ` +
                    `VU=${__VU} | ` +
                    `Status=${response.status} | ` +
                    `OrderId=${orderId} | ` +
                    `Body=${response.body}`
                );
            }

            break;
    }

    check(response, {

        '500 hatasi yok':
            (r) => r.status < 500,

        'payment basarili':
            (r) => r.status === 200
    });
}