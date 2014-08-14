<html>
<head>
<title>Sample PHP application for Wild Apricot API</title>
</head>
<body>
<p>
This application demonstrates how to use Wild Apricot API v2.

See http://help.wildapricot.com/display/DOC/API+Version+2 for detailed description of API.
<p>
<hr>
<?php
  
    $apiKey = ''; //Insert your API key here
    $adminLogin = ''; //insert admin's email here
    $adminPassword = ''; //insert admin's password here

    $allScopes = "general_info contacts finances events event_registrations account membership_levels settings";

    // uncomment only one line of these two
    $token = getAuthTokenByAdminCredentials($adminLogin, $adminPassword);
    //$token = getAuthTokenByApiKey( $apiKey);
    
    $accountUrl = getAccountDetails()['Url'];
    $contacts = getContactsList()['Contacts'];
    foreach ($contacts as $contact) {
        echo sprintf("#%d - %s", $contact['Id'], $contact['DisplayName']);        
        echo '<br>';
    }

    function getAccountDetails(){
        $url = 'https://api.wildapricot.org/v2/Accounts/';
        return makeAuthenticatedRequest($url)[0];  // usually you have access to one account
    }
    
    function getContactsList() {
        global $accountUrl;  
       
        $queryParams = array('$async'=>'false',      // execute request synchronously
              '$top'=>10,                           // limit result set to 10 records
              '$filter'=>'Member eq true',          // filter only members
              '$select'=>'First name, Last name');  // select only first name and last name to reduce size of json data
              
        $url = $accountUrl . '/Contacts/?' . http_build_query($queryParams);   
        return makeAuthenticatedRequest($url);      
    }
    
    // this function makes authenticated request to API
    // -----------------------
    // $url is an absolute URL
    // $verb is an optional parameter. 
    // Use 'GET' to retrieve data,
    //     'POST' to create new record
    //     'PUT' to update existing record
    //     'DELETE' to remove record
    // $data is an optional parameter - data to sent to server. Pass this parameter with 'POST' or 'PUT' requests.
    // ------------------------
    // returns object decoded from response json 
    function makeAuthenticatedRequest($url, $verb='GET', $data=null) {
        global $token;

        $crl = curl_init();
        $headers = array();
        $headers[] = "Authorization: Bearer " . $token;
        $headers[] = "Content-Type: application/json";

        curl_setopt($crl, CURLOPT_URL, $url);
        curl_setopt($crl, CURLOPT_HTTPHEADER, $headers);

        
        curl_setopt($crl, CURLOPT_CUSTOMREQUEST, $verb);
        if($data)
        {
            $jsonData = json_encode($data);
            curl_setopt($crl, CURLOPT_POSTFIELDS, $jsonData);
        }
        
        curl_setopt($crl, CURLOPT_RETURNTRANSFER, true);    

        $jsonResult = curl_exec($crl);
        curl_close($crl);
        
        // uncomment to debug
        //echo $jsonResult;
        $result = json_decode( $jsonResult , true );
        
        return $result;
    }
    
    function getAuthTokenByAdminCredentials($login, $password)
    {
        global $allScopes;
        
        if($login == ''){
            throw new Exception('login is empty');
        }
        
        $data = sprintf("grant_type=%s&username=%s&password=%s&scope=%s", 'password', $login, $password, $allScopes);
        $authorizationHeader = "Authorization: Basic " . base64_encode( "SamplePhpApplication:open_wa_api_client");
        $result = getAuthToken($data, $authorizationHeader);
        return $result['access_token'];      
    }
        
    function getAuthTokenByApiKey($apiKey) {
        global $allScopes;
        
        if($apiKey == ''){
            throw new Exception('apiKey is empty');
        }
        
        $data = sprintf("grant_type=%s&scope=%s", 'client_credentials', $allScopes);
        $authorizationHeader = "Authorization: Basic " . base64_encode("APIKEY:" . $apiKey);
        $result = getAuthToken($data, $authorizationHeader);
        return $result['access_token'];     
    }
    
    function getAuthToken($data, $authorizationHeader)
    {
        $crl = curl_init();
        $headers = array();
        $headers[] = $authorizationHeader;
        $headers[] = "Content-Length: " . strlen($data);
        curl_setopt($crl, CURLOPT_URL, "https://oauth.wildapricot.org/auth/token");
        curl_setopt($crl, CURLOPT_HTTPHEADER, $headers);
        curl_setopt($crl, CURLOPT_POST, true);
        curl_setopt($crl, CURLOPT_POSTFIELDS, $data);
        curl_setopt($crl, CURLOPT_RETURNTRANSFER, true);
        $result = json_decode(  curl_exec($crl), true );        
        curl_close($crl);
        return $result;      
    }
?>
</body>
</html>



