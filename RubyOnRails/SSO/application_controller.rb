class ApplicationController < ActionController::Base
  protect_from_forgery with: :exception
  #run this code on all pages
  before_action :SSOAllPages
  def SSOAllPages
    #base64 to encode the clientID and secret code, JSON for parsing to JSON
    require "base64"
    require "json"
    #check if there are any url queries, if there aren't any, redirect to login
    if URI.parse(request.original_url).query == nil
        puts "No code, redirecting"
        #format this with your Wild Apricot domain name, your Client ID and the URI of your external site, can be localhost for testing
        redirect_to "http://www.SITE.wildapricot.org/sys/login/OAuthLogin?client_id=CLIENTID&redirect_uri=EXTERNALSITE&scope=contacts_me"
    else #if there is a query in the url, assume it is a code and try to authenticate
      #get the queries from the URL
      siteResponseParameters = CGI.parse(URI.parse(request.original_url).query)
      #get the Wild Apricot oauth server as https
      oAuthURI = URI.parse("https://oauth.wildapricot.org")
      oAuthServer = Net::HTTP.new(oAuthURI.host,oAuthURI.port)
      oAuthServer.use_ssl = true
      #setup headers for the POST
      postHeaders = {
        'Content-Type' => 'application/x-www-form-urlencoded',
        'Authorization' => 'Basic ' + Base64.encode64('CLIENTID:SECRETCODE')} #replace with your client ID and secret code
      #set up queries for the post, format this with your client ID and your external site URI
      postStringQueries = 'grant_type=authorization_code&code=' + siteResponseParameters['code'].first + '&client_id=CLIENTID&redirect_uri=EXTERNALSITE&scope=contacts_me'
      #post to the Wild Apricot oauth server and store the response
      oAuthResponse = oAuthServer.post('/auth/token',postStringQueries,postHeaders)
      #convert the body of the response into JSON and print the currently logged in account ID
      oAuthJSON = JSON.parse(oAuthResponse.body)
      puts oAuthJSON["Permissions"][0]['AccountId']
    end
  end
end
