var authServiceUrl = 'https://oauth.wildapricot.org/auth/token';
var apiUrl = 'https://api.wildapricot.org';

function onOpen() {   
  SpreadsheetApp.getUi()
      .createMenu('Wild Apricot')
      .addItem('Get account details', 'getAccountDetails')
      .addToUi();
};
function getAccountDetails(){   
  Logger.clear();
   
  var ui = SpreadsheetApp.getUi();  
  var sheet = SpreadsheetApp.getActiveSpreadsheet().getActiveSheet();
  
  var token = getTokenByAdminCredentials(ui);
  //var token = getTokenByApiKey(ui);
  
  var account = getDataFromApi(apiUrl+'/v2/accounts', token)[0];
   
  sheet.getRange('B4').setValue(account.Id);
  sheet.getRange('B5').setValue(account.Name);
  sheet.getRange('B6').setValue(account.PrimaryDomainName);
  sheet.getRange('B7').setValue(account.ContactLimitInfo.CurrentContactsCount);
  sheet.getRange('B8').setValue(account.ContactLimitInfo.BillingPlanContactsLimit);  
}

function getTokenByAdminCredentials(ui){
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

function getTokenByApiKey(ui){
  var ui = SpreadsheetApp.getUi();  
  var sheet = SpreadsheetApp.getActiveSpreadsheet().getActiveSheet();
  
  userResponse = ui.prompt("Enter api key");
  if( userResponse.getSelectedButton() != ui.Button.OK ) return;
  var apiKey = userResponse.getResponseText();
   
  var token = retrieveTokenByApiKey(authServiceUrl, apiKey);
  return token;
}
   
function getDataFromApi(url, token)
{
  var requestParams = {
    method:'GET',
    headers: { Authorization:'Bearer ' + token },
    accept:'application/json'
  }
  var responseJson = UrlFetchApp.fetch(url, requestParams);
  return JSON.parse(responseJson);
}
function retrieveTokenByAdminCredentials(authServiceUrl, login, password)
{
  var clientId = "myGoogleScriptApp";
  var clientSecret = "open_wa_api_client";
  var scopeNames = "general_info contacts finances events event_registrations account membership_levels settings";
   
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
  var clientId = "sample_google_script_app";
  var clientSecret = "open_wa_api_client";
  var scopeNames = "general_info contacts finances events event_registrations account membership_levels settings";
  
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