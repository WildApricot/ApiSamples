const request = require('request');
const oauthTokenUrl = 'https://oauth.wildapricot.org/auth/token';
const apiBaseUrl = 'https://api.wildapricot.org/v2/';

// these api key and accountId are related to demo account houseofbamboo.wildapricot.org
// It provides read-only access to demo content
// please replace it with your own values

const api_key = 'ebyvoeo6fauigr1w7h3mbhd7ra93dh';
const accountId = 183112;

request.post(oauthTokenUrl, {
  form: {
    grant_type: 'client_credentials',
    scope: 'auto'
  },
  headers: {
     'content-type': 'application/x-www-form-urlencoded',
     'Accept': 'application/json',
     "Authorization": "Basic " + new Buffer(`APIKEY:${api_key}`).toString('base64')
   },
  json: true
}, function (err, res, body) {
  if( err) {
    console.error(err);
    throw err;
  }
  else {
    listContacts(body.access_token, "Johnette");
  }
})

function listContacts(authToken, searchString) {
  request.get(`${apiBaseUrl}/accounts/${accountId}/contacts`, {
      headers: {
        'Accept': 'application/json',
        "Authorization": `Bearer ${authToken}`
      },
      qs: {
        $async: false,
        $filter: `'First name' eq '${searchString}'`,
        $select: "'First name'"
      },
      json: true
    },
    function( err, res, body) {
      if( err) {
        console.error(err);
        throw err;
      }
      else {
        for (var i = 0; i < body.Contacts.length; i++) {
          let contact = body.Contacts[i];
          console.log(`${contact.DisplayName}: ${contact.Email}`);
        }
      }
    }
  );
}
