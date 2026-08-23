import http from 'k6/http';
import { check } from 'k6';
import { Counter } from 'k6/metrics';

const serverErrors = new Counter('server_errors');
const successResponses = new Counter('success_responses');
const duplicateResponses = new Counter('duplicate_responses');

export const options = {
    insecureSkipTLSVerify: true,

    scenarios: {
        idempotency_race: {
            executor: 'per-vu-iterations',

            // 100 eþzamanlý kullanýcý
            vus: 100,

            // Her kullanýcý sadece 1 payment isteði atacak
            iterations: 1,

            maxDuration: '30s'
        }
    },

    thresholds: {
        server_errors: ['count==0']
    }
};

const BASE_URL = 'https://localhost:7166';


// Test baþlamadan önce SADECE 1 kez çalýþýr
export function setup() {

    const tokenPayload = JSON.stringify({
        clientCode: 'mock-integration',
        clientSecret: 'mock-secret-123'
    });

    const tokenResponse = http.post(
        `${BASE_URL}/api-integration-tokens`,
        tokenPayload,
        {
            headers: {
                'Content-Type': 'application/json'
            }
        }
    );

    console.log(`Token status: ${tokenResponse.status}`);
    console.log(`Token response: ${tokenResponse.body}`);

    if (
        tokenResponse.status < 200 ||
        tokenResponse.status >= 300
    ) {
        throw new Error(
            `Token alýnamadý. Status: ${tokenResponse.status}`
        );
    }

    const body = tokenResponse.json();

    // Token response modelini henüz görmediðimiz için
    // olasý alanlarý kontrol ediyoruz.
    const token =
        body.accessToken ||
        body.token ||
        body.data?.accessToken ||
        body.data?.token;

    if (!token) {
        throw new Error(
            `Token response içinde token bulunamadý: ${tokenResponse.body}`
        );
    }

    console.log('Token baþarýyla alýndý.');

    return {
        token: token
    };
}


export default function (data) {

    /*
        BÝLEREK bütün VU'lar ayný request'i gönderiyor.

        Böylece backend ayný IdempotencyKey'i üretmeli.
    */
    const paymentPayload = JSON.stringify({
        integrationClientId:
            '11111111-1111-1111-1111-111111111111',

        clientCode:
            'mock-integration',

        merchantCode:
            'MOCK-MERCHANT',

        orderId:
            'LOAD-IDEMPOTENCY-RACE-001',

        amount:
            10,

        currency:
            'TRY',

        providerCode:
            'Mock',

        channel:
            'ECommerce'
    });

    const paymentResponse = http.post(
        `${BASE_URL}/api/payments`,
        paymentPayload,
        {
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${data.token}`
            },

            tags: {
                test_type: 'idempotency-race'
            }
        }
    );


    // Baþarýlý payment veya completed response replay
    if (
        paymentResponse.status >= 200 &&
        paymentResponse.status < 300
    ) {
        successResponses.add(1);
    }


    // InFlight duplicate response
    if (
        paymentResponse.status === 400 ||
        paymentResponse.status === 409
    ) {
        duplicateResponses.add(1);
    }


    // Özellikle kontrol etmek istediðimiz durum
    if (paymentResponse.status >= 500) {

        serverErrors.add(1);

        console.error(
            `SERVER ERROR | ` +
            `VU=${__VU} | ` +
            `status=${paymentResponse.status} | ` +
            `body=${paymentResponse.body}`
        );
    }


    check(paymentResponse, {

        '500 hatasi yok':
            (r) => r.status < 500,

        'request kontrollu handle edildi':
            (r) =>
                (r.status >= 200 && r.status < 300) ||
                r.status === 400 ||
                r.status === 409
    });
}