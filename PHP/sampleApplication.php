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
  
    $waApiClient = WaApiClient::getInstance();

  // uncomment only one line of these two
    // $waApiClient->initTokenByApiKey('put_your_apikey_here');
    $waApiClient->initTokenByContactCredentials('admin@yourdomain.com', 'your_password');
  
    $account = getAccountDetails();
    $accountUrl = $account['Url'];
    
    $contactsResult = getContactsList(); 
    $contacts =  $contactsResult['Contacts'];
    echo "<p>Total Contacts Count: " . count($contacts) . "</p>";

    echo "<p>First 10 contacts:</p>";
   $counter = 0;
   foreach($contacts as $contact) {
      if ($counter >= 10) break; // stop after 10 contacts

      echo '<br />';
      echo sprintf("#%d - %s", $contact['Id'], $contact['DisplayName']);

      $counter++;
   }

    function getAccountDetails()
    {
       global $waApiClient;
       $url = 'https://api.wildapricot.org/v2/Accounts/';
       $response = $waApiClient->makeRequest($url); 
       return  $response[0]; // usually you have access to one account
    }

    function getContactsList()
    {
       global $waApiClient;
       global $accountUrl;

       $top = 100;
       $skip = 0;
       $allContacts = array();

       while (true) {
          $queryParams = array(
             '$async'  => 'false', // execute request synchronously
             '$top'    => $top,    // page size (API limit)
             '$skip'   => $skip,   // paging offset
            // '$filter' => 'Member eq true', // keep original filter (remove or change if not needed)
             '$select' => 'First name, Last name' // keep selection to reduce payload (adjust as needed)
          );

          $url = $accountUrl . '/Contacts/?' . http_build_query($queryParams);
          echo "<p>Requesting: $url</p>";
          $response = $waApiClient->makeRequest($url);

          // response may contain 'Contacts' property or be the contacts array directly
          $page = isset($response['Contacts']) ? $response['Contacts'] : $response;

          if (empty($page)) {
             break;
          }

          // merge page into accumulator
          $allContacts = array_merge($allContacts, $page);

          // if returned less than requested page size, no more records
          if (count($page) < $top) {
             break;
          }

          $skip += $top;
       }

       // return in the same shape the rest of the code expects
       return array('Contacts' => $allContacts);
    }

    /**
     * API helper class. You can copy whole class in your PHP application.
     */
    class WaApiClient
    {
       const AUTH_URL = 'https://oauth.wildapricot.org/auth/token';
             
       private $tokenScope = 'auto';

       private static $_instance;
       private $token;
       
       public function initTokenByContactCredentials($userName, $password, $scope = null)
       {
          if ($scope) {
             $this->tokenScope = $scope;
          }

          $this->token = $this->getAuthTokenByAdminCredentials($userName, $password);
          if (!$this->token) {
             throw new Exception('Unable to get authorization token.');
          }
       }

       public function initTokenByApiKey($apiKey, $scope = null)
       {
          if ($scope) {
             $this->tokenScope = $scope;
          }

          $this->token = $this->getAuthTokenByApiKey($apiKey);
          if (!$this->token) {
             throw new Exception('Unable to get authorization token.');
          }
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

       public function makeRequest($url, $verb = 'GET', $data = null)
       {
          if (!$this->token) {
             throw new Exception('Access token is not initialized. Call initTokenByApiKey or initTokenByContactCredentials before performing requests.');
          }

          $ch = curl_init();
          curl_setopt($ch, CURLOPT_URL, $url);
          curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
          curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
          curl_setopt($ch, CURLOPT_SSL_VERIFYHOST, false);

          $headers = array(
             'Authorization: Bearer ' . $this->token,
             'Content-Type: application/json'
          );
          curl_setopt($ch, CURLOPT_URL, $url);
          
          if ($data) {
             $jsonData = json_encode($data);
             curl_setopt($ch, CURLOPT_POSTFIELDS, $jsonData);
             $headers = array_merge($headers, array('Content-Length: '.strlen($jsonData)));
          }
          curl_setopt($ch, CURLOPT_HTTPHEADER, $headers);
          curl_setopt($ch, CURLOPT_CUSTOMREQUEST, $verb);

          curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
          $jsonResult = curl_exec($ch);
          if ($jsonResult === false) {
             throw new Exception(curl_errno($ch) . ': ' . curl_error($ch));
          }

          // var_dump($jsonResult); // Uncomment line to debug response

          curl_close($ch);
          return json_decode($jsonResult, true);
       }

       private function getAuthTokenByAdminCredentials($login, $password)
       {
          if ($login == '') {
             throw new Exception('login is empty');
          }

          $data = sprintf("grant_type=%s&username=%s&password=%s&scope=%s", 'password', urlencode($login), urlencode($password), urlencode($this->tokenScope));

          throw new Exception('Change clientId and clientSecret to values specific for your authorized application. For details see:  https://help.wildapricot.com/display/DOC/Authorizing+external+applications');
          $clientId = 'SamplePhpApplication';
          $clientSecret = 'open_wa_api_client';
          $authorizationHeader = "Authorization: Basic " . base64_encode( $clientId . ":" . $clientSecret);

          return $this->getAuthToken($data, $authorizationHeader);
       }

       private function getAuthTokenByApiKey($apiKey)
       {
          $data = sprintf("grant_type=%s&scope=%s", 'client_credentials', $this->tokenScope);
          $authorizationHeader = "Authorization: Basic " . base64_encode("APIKEY:" . $apiKey);
          return $this->getAuthToken($data, $authorizationHeader);
       }

       private function getAuthToken($data, $authorizationHeader)
       {
          $ch = curl_init();
          curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
          curl_setopt($ch, CURLOPT_SSL_VERIFYHOST, false);
         echo "<p>Requesting auth token...</p>";
          $headers = array(
             $authorizationHeader,
             'Content-Length: ' . strlen($data)
          );
          curl_setopt($ch, CURLOPT_URL, WaApiClient::AUTH_URL);
          curl_setopt($ch, CURLOPT_HTTPHEADER, $headers);
          curl_setopt($ch, CURLOPT_POST, true);
          curl_setopt($ch, CURLOPT_POSTFIELDS, $data);
          curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
		  $response = curl_exec($ch);
		  if ($response === false) {
             throw new Exception(curl_errno($ch) . ': ' . curl_error($ch));
          }
          // var_dump($response); // Uncomment line to debug response	
		  

        $result = json_decode($response , true);
          curl_close($ch);

          return $result['access_token'];
       }

       public static function getInstance()
       {
          if (!is_object(self::$_instance)) {
             self::$_instance = new self();
          }

          return self::$_instance;
       }

       public final function __clone()
       {
          throw new Exception('It\'s impossible to clone singleton "' . __CLASS__ . '"!');
       }

       private function __construct()
       {
          if (!extension_loaded('curl')) {
             throw new Exception('cURL library is not loaded');
          }
       }

       public function __destruct()
       {
          $this->token = null;
       }
    }
?>
</body>
</html>
