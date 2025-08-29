function onOpen() {
  
  SpreadsheetApp.getUi()
      .createMenu('Wild Apricot')
      .addItem('Get account details', 'getAccountDetails')
      .addItem('Show me some contact magic', 'createNewContact')
      .addItem('Get Contacts', 'getContacts')
      .addToUi();
};

function getAccountDetails(){  
  Logger.clear();
  
  var ui = SpreadsheetApp.getUi();  
  var sheet = SpreadsheetApp.getActiveSpreadsheet().getActiveSheet();
  var urls = urlBuilder();
 
  var token = getToken(ui, urls.getAuthServiceUrl());
  var account = getDataFromApi(urls.getAccountsListUrl(), token)[0];
  
  sheet.getRange('B4').setValue(account.Id);
  sheet.getRange('B5').setValue(account.Name);
  sheet.getRange('B6').setValue(account.PrimaryDomainName);
  sheet.getRange('B7').setValue(account.ContactLimitInfo.CurrentContactsCount);
  sheet.getRange('B8').setValue(account.ContactLimitInfo.BillingPlanContactsLimit);  
}

function getContacts(){  
  Logger.clear();
  
  var urls = urlBuilder();
  var ui = SpreadsheetApp.getUi();  
  var sheet = SpreadsheetApp.getActiveSpreadsheet().insertSheet();
  var token = getToken(ui, urls.getAuthServiceUrl());
  var accountId = getDataFromApi(urls.getAccountsListUrl(), token)[0].Id;

  
  var urlBase = urls.getContactsListUrl(accountId) + '/?$async=false';
  var pageSize = 10;
  var skip = 0;
  var allContacts = [];

  // Fetch all pages
  while (true) {
    var url = urlBase + "&$top=" + pageSize + "&$skip=" + skip;
    var result = executeApiRequest(url, token, "GET"); // array of contacts
    batch = result.Contacts
    if (!batch || batch.length === 0) break;

    allContacts = allContacts.concat(batch);
    skip += pageSize;
  }

  if (allContacts.length === 0) {
    SpreadsheetApp.getUi().alert("No contacts found.");
    return;
  }

  // Collect all unique field names from FieldValues
  var fieldNamesSet = new Set();
  allContacts.forEach(function(c) {
    if (c.FieldValues && Array.isArray(c.FieldValues)) {
      c.FieldValues.forEach(function(fv) {
        fieldNamesSet.add(fv.FieldName);
      });
    }
  });
  var fieldNames = Array.from(fieldNamesSet);

  // Final headers: Id, Name + all field names
  var headers = ["Id", "Name"].concat(fieldNames);
  sheet.getRange(1, 1, 1, headers.length).setValues([headers]);

  // Build rows
  var values = allContacts.map(function(c) {
    var row = [c.Id, c.Name];

    // map each fieldName → value
    var fieldMap = {};
    if (c.FieldValues) {
      c.FieldValues.forEach(function(fv) {
        fieldMap[fv.FieldName] = fv.Value;
      });
    }

    // fill values in same order as headers
    fieldNames.forEach(function(fn) {
      row.push(fieldMap[fn] !== undefined ? fieldMap[fn] : "");
    });

    return row;
  });

  // Write rows
  sheet.getRange(2, 1, values.length, headers.length).setValues(values);
}

function createNewContact()
{
  Logger.clear();
  
  var urls = urlBuilder();
  var ui = SpreadsheetApp.getUi();  
  var sheet = SpreadsheetApp.getActiveSpreadsheet().getActiveSheet();

  var token = getToken(ui, urls.getAuthServiceUrl());
  var accountId = getDataFromApi(urls.getAccountsListUrl(), token)[0].Id;
  var contactFields = getDataFromApi(urls.getContactFieldsUrl(accountId), token);
  
  var newContact = { fieldValues:[] };
  
  contactFields.forEach(function(element, index){
    var fieldDescription = element;
    
    if( fieldDescription.IsEditable && // only editable fields can be modified
       fieldDescription.SystemCode != 'Email' && // api validates email format, so we can't use sample random string
       fieldDescription.Type == 'String' ) // just for example filter only string fields
    {
      newContact.fieldValues.push( { FieldName:fieldDescription.FieldName, Value: getSomeRandomString() })
    }
  });
  
  var createdContact = executeApiRequest(urls.getContactsListUrl(accountId),token,'POST', newContact);
  ui.alert('contact #' + createdContact.Id + ' successfully created');
}

function getToken(ui, authServiceUrl)
{
  // var token = getTokenByAdminCredentials(ui, authServiceUrl);
  var token = getTokenByApiKey(ui, authServiceUrl);
  return token;
}

function getTokenByApiKey(ui, authServiceUrl){
  var sheet = SpreadsheetApp.getActiveSpreadsheet().getActiveSheet();
  
  userResponse = ui.prompt("Enter api key");
  if( userResponse.getSelectedButton() != ui.Button.OK ) return;
  var apiKey = userResponse.getResponseText();
   
  var token = retrieveTokenByApiKey(authServiceUrl, apiKey);
  return token;
}

function getTokenByAdminCredentials(ui, authServiceUrl){
  var ui = SpreadsheetApp.getUi();  
  var sheet = SpreadsheetApp.getActiveSpreadsheet().getActiveSheet();
  
  userResponse = ui.prompt("Enter account administrator's login");
  if( userResponse.getSelectedButton() != ui.Button.OK ) return;
  var login = userResponse.getResponseText();
   
  userResponse = ui.prompt("Enter password");
  if( userResponse.getSelectedButton() != ui.Button.OK ) return;
  var password = userResponse.getResponseText();
  
  var token = retrieveTokenByAdminCredentials(authServiceUrl, login, password);
  return token;
}
  
function getDataFromApi(url, token)
{
  return executeApiRequest(url, token, 'GET');
}

function executeApiRequest(url, token, method, data)
{
  if (typeof method === 'undefined') { method = 'GET'; }
  
   var requestParams = {
    method:method,
    headers: { Authorization:'Bearer ' + token },
    accept:'application/json'
  }
   
  if( data )
  {
    requestParams.payload = JSON.stringify(data);
    requestParams.contentType = 'application/json';    
  }
   
  var responseJson = UrlFetchApp.fetch(url, requestParams);
  var result = JSON.parse(responseJson);
  Logger.log(result);
  return result;
}

function retrieveTokenByAdminCredentials(authServiceUrl, login, password)
{
  throw 'change clientId and clientSecret to values specific for your authirized application. For details see:  https://help.wildapricot.com/display/DOC/Authorizing+external+applications';
  var clientId = "myGoogleScriptApp"; 
  var clientSecret = "open_wa_api_client";
  var scopeNames = "auto";
   
  var authRequestParams = {
    method:'POST',
    headers:{
      Authorization:'Basic ' + Utilities.base64Encode(clientId + ':' + clientSecret)
    },
    contentType: 'application/x-www-form-urlencoded',
    payload: Utilities.formatString('grant_type=%s&username=%s&password=%s&scope=%s', 'password', login, password, scopeNames)
  };
  var tokenJson = UrlFetchApp.fetch(authServiceUrl, authRequestParams);
  var tokenData = JSON.parse(tokenJson);
  return tokenData.access_token;
}

function retrieveTokenByApiKey(authServiceUrl, apiKey)
{
  var scopeNames = "contacts finances events event_registrations account membership_levels";
  
  var authRequestParams = {
    method:'POST',
    headers:{
      Authorization:'Basic ' + Utilities.base64Encode('APIKEY' + ':' + apiKey)
    },
    contentType: 'application/x-www-form-urlencoded',
    payload: Utilities.formatString('grant_type=%s&scope=%s', 'client_credentials', scopeNames)
  };

  var tokenJson = UrlFetchApp.fetch(authServiceUrl, authRequestParams);
  var tokenData = JSON.parse(tokenJson);
  return tokenData.access_token;
}

function getSomeRandomString()
{
  var src = 'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum';
  return src.substring(Math.floor(1+Math.random()*100), 20);
}

function urlBuilder()
{
  var apiUrl = 'https://api.wildapricot.org';
  return {
    getAuthServiceUrl: function()
    {
      return 'https://oauth.wildapricot.org/auth/token';
    },
    getAccountsListUrl: function()
    {
      return apiUrl+'/v2/accounts';
    },
    getContactFieldsUrl: function(accountId)
    {
      return apiUrl+'/v2/accounts/' + accountId + '/contactfields';
    },
    getContactsListUrl: function(accountId)
    {
      return apiUrl+'/v2/accounts/' + accountId + '/contacts';
    }
  };
}