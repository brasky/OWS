import http from 'k6/http';
import { check, group, fail } from 'k6';
import { sleep } from 'k6'

export let options = {
    insecureSkipTLSVerify: true,
    noConnectionReuse: false,
    stages: [
        { duration: '10s', target: 10},
        { duration: '10s', target: 10},
        { duration: '10s', target: 1000},
        { duration: '10s', target: 1000},
        { duration: '2s', target: 10},
        { duration: '2s', target: 10}
    ]
};

// Create a random string of given length
function randomString(length, charset = '') {
    if (!charset) charset = 'abcdefghijklmnopqrstuvwxyz';
    let res = '';
    while (length--) res += charset[(Math.random() * charset.length) | 0];
    return res;
}

const USERNAME = `${randomString(10)}@example.com`; // Set your own email or `${randomString(10)}@example.com`;
const PASSWORD = 'superCroc2024';

const BASE_URL = 'https://localhost:44303/api';

// Register a new user and retrieve authentication token for subsequent API requests
export function setup() {
    let data = {
        FirstName: 'Crocodile',
        LastName: 'Owner',
        Email:USERNAME,
        Password: PASSWORD
    };

    const res = http.post(`${BASE_URL}/users/RegisterUser`, JSON.stringify(data), {
        headers: { 'Content-Type': 'application/json' }
    });
    check(res, { 'created user': (r) => r.json('authenticated') === true });

    const userSessionGuid = res.json('userSessionGuid');
    check(userSessionGuid, { 'logged in successfully': () => userSessionGuid !== '' });

    return userSessionGuid;
}


export default (userSessionGuid) => {
    // set the authorization header on the session for the subsequent requests
    const requestConfigWithTag = (tag) => ({
        headers: {
            'X-CustomerGUID': 'E2FED99F-2F3A-4BFB-AB00-A586B92B5549',
            'Content-Type': 'application/json'
        },
        tags: Object.assign(
            {},
            {
                name: 'PrivateCrocs',
            },
            tag
        ),
    });

    let URL = `${BASE_URL}`;
    group('01. GetUserSession', () => {
        const res = http.get(`${URL}/users/GetUserSession?UserSessionGUID=${userSessionGuid}`, requestConfigWithTag({ name: 'Create' }));

        if (check(res, { 'Got user session!': (r) => r.status === 200 })) {
            // URL = `${URL}${res.json('id')}/`;
        } else {
            console.log(`Unable to get user session ${res.status} ${res.body}`);
            return;
        }
    });
    
    sleep(1)

    group('02. Get All Characters (empty)', () => {
        const payload = {
                UserSessionGUID: userSessionGuid
            };
        const res = http.post(`${BASE_URL}/users/GetAllCharacters`, JSON.stringify(payload), requestConfigWithTag({ name: 'Get' }));
        check(res, { 'retrieved characters': (r) => r.status === 200 });
        //Should be 0 because we just made our account
        check(res.json(), { 'retrieved characters': (r) => r.length === 0 });
    });
    
    sleep(1)

    const newCharacterPayload = {
        UserSessionGUID: userSessionGuid,
        CharacterName: randomString(8),
        ClassName: "MaleWarrior"
    };

    sleep(1)
    
    group('03. Create a character', () => {
        const res = http.post(`${BASE_URL}/users/CreateCharacter`, JSON.stringify(newCharacterPayload), requestConfigWithTag({ name: 'Create' }));
        const isSuccessfulUpdate = check(res, {
            'Character name is correct': () => res.json('characterName') === newCharacterPayload.CharacterName,
        });

        if (!isSuccessfulUpdate) {
            console.log(`Unable to create character ${res.status} ${res.body}`);
            return;
        }
    });
    sleep(1)
    
    group('04. Get All Characters (1)', () => {
        const payload = {
                UserSessionGUID: userSessionGuid
            };
        const res = http.post(`${BASE_URL}/users/GetAllCharacters`, JSON.stringify(payload), requestConfigWithTag({ name: 'Get' }));
        check(res, { 'retrieved characters': (r) => r.status === 200 });
        //Should be 1 because we just made a character
        check(res.json(), { 'retrieved characters': (r) => r.length === 1 });
    });

    sleep(1)

    group('05. Delete the character', () => {
        const payload = {
            UserSessionGUID: userSessionGuid,
            CharacterName: newCharacterPayload.CharacterName,
        };
        const res = http.post(`${BASE_URL}/users/RemoveCharacter`, JSON.stringify(payload), requestConfigWithTag({ name: 'Delete' }));
        check(res.json(), { 'Delete Success': (r) => r.success === true });
        const isSuccessfulDelete = check(null, {
            'Character was deleted correctly': () => res.status === 200,
        });

        if (!isSuccessfulDelete) {
            console.log(`Character was not deleted properly`);
            return;
        }
    });

    sleep(1)

    group('06. Logout', () => {
        const payload = {
            UserSessionGUID: userSessionGuid,
        };
        const res = http.post(`${BASE_URL}/users/Logout`, JSON.stringify(payload), requestConfigWithTag({ name: 'Logout' }));
        check(null, {
            'logout was successful': () => res.status === 200,
        });

    });

};