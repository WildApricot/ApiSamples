const axios = require('axios');
const config = require('./config');

async function fetchContacts(access_token, skip = 0, top = 100) {
    const response = await axios.get(
        `${config.urls.apiUrl}/${config.credentials.accountId}/contacts/?$async=false&$top=${top}&$skip=${skip}`, 
        {
            headers: {
                'Authorization': 'Bearer ' + access_token
            }
        }
    );
    return response.data.Contacts;
}

async function main() {
    try {
        console.log('Get authorization token...');
        // Get authorization token
        const authResponse = await axios.post(config.urls.auth, 
            'grant_type=client_credentials&scope=auto',
            {
                headers: {
                    'Authorization': 'Basic ' + Buffer.from('APIKEY:' + config.credentials.apiKey).toString('base64'),
                    'Content-Type': 'application/x-www-form-urlencoded'
                }
            }
        );

        const access_token = authResponse.data.access_token;
        console.log('Access Token:', access_token);

        // Fetch all contacts with pagination
        let allContacts = [];
        let skip = 0;
        const batchSize = 100; // Define batch size as a constant
        let hasMoreRecords = true;

        while (hasMoreRecords) {
            console.log(`Fetching contacts ${skip} to ${skip + batchSize}...`);
            const contacts = await fetchContacts(access_token, skip, batchSize);
            
            if (contacts.length === 0) {
                hasMoreRecords = false;
            } else {
                allContacts = allContacts.concat(contacts);
                skip += batchSize;
            }
        }

        console.log('Total contacts found:', allContacts.length);
        
        // Get last 5 contacts
        const lastFiveContacts = allContacts.slice(-5);
        console.log('\nLast 5 contacts:');
        lastFiveContacts.forEach((contact, index) => {
            console.log(`${index + 1}. Name: ${contact.FirstName} ${contact.LastName}, Email: ${contact.Email}`);
        });

    } catch (error) {
        console.error('Error:', error.message);
        if (error.response) {
            console.error('Response data:', error.response.data);
        }
    }
}

main();
