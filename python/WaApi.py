r"""
This module provides set of classes for working with WildApricot public API v2.
Public API documentation can be found here: http://help.wildapricot.com/display/DOC/API+Version+2

Example:
    api = WaApi.WaApiClient()
    api.authenticate_with_contact_credentials("admin@youraccount.com", "your_password")
    accounts = api.execute_request("/v2/accounts")
    for account in accounts:
        print(account.PrimaryDomainName)
"""

__author__ = 'dsmirnov@wildapricot.com'

import datetime
import urllib.request
import urllib.response
import urllib.error
import urllib.parse
import json
import base64


class WaApiClient(object):
    """Wild apricot API client."""
    auth_endpoint = "https://oauth.wildapricot.org/auth/token"
    api_endpoint = "https://api.wildapricot.org"
    _token = None
    client_id = None
    client_secret = None

    def __init__(self, client_id, client_secret):
        self.client_id = client_id
        self.client_secret = client_secret

    def authenticate_with_apikey(self, api_key, scope=None):
        """perform authentication by api key and store result for execute_request method

        api_key -- secret api key from account settings
        scope -- optional scope of authentication request. If None full list of API scopes will be used.
        """
        scope = "auto" if scope is None else scope
        data = {
            "grant_type": "client_credentials",
            "scope": scope
        }
        encoded_data = urllib.parse.urlencode(data).encode()
        request = urllib.request.Request(self.auth_endpoint, encoded_data, method="POST")
        request.add_header("ContentType", "application/x-www-form-urlencoded")
        request.add_header("Authorization", 'Basic ' + base64.standard_b64encode(('APIKEY:' + api_key).encode()).decode())
        response = urllib.request.urlopen(request)
        self._token = WaApiClient._parse_response(response)
        self._token.retrieved_at = datetime.datetime.now()

    def authenticate_with_contact_credentials(self, username, password, scope=None):
        """perform authentication by contact credentials and store result for execute_request method

        username -- typically a contact email
        password -- contact password
        scope -- optional scope of authentication request. If None full list of API scopes will be used.
        """
        scope = "auto" if scope is None else scope
        data = {
            "grant_type": "password",
            "username": username,
            "password": password,
            "scope": scope
        }
        encoded_data = urllib.parse.urlencode(data).encode()
        request = urllib.request.Request(self.auth_endpoint, encoded_data, method="POST")
        request.add_header("ContentType", "application/x-www-form-urlencoded")
        auth_header = base64.standard_b64encode((self.client_id + ':' + self.client_secret).encode()).decode()
        request.add_header("Authorization", 'Basic ' + auth_header)
        response = urllib.request.urlopen(request)
        self._token = WaApiClient._parse_response(response)
        self._token.retrieved_at = datetime.datetime.now()

    def execute_request(self, api_url, api_request_object=None, method=None):
        """
        perform api request and return result as an instance of ApiObject or list of ApiObjects

        api_url -- absolute or relative api resource url
        api_request_object -- any json serializable object to send to API
        method -- HTTP method of api request. Default: GET if api_request_object is None else POST
        """
        if self._token is None:
            raise ApiException("Access token is not abtained. "
                               "Call authenticate_with_apikey or authenticate_with_contact_credentials first.")

        if not api_url.startswith("http"):
            api_url = self.api_endpoint + api_url

        if method is None:
            if api_request_object is None:
                method = "GET"
            else:
                method = "POST"

        request = urllib.request.Request(api_url, method=method)
        if api_request_object is not None:
            request.data = json.dumps(api_request_object, cls=_ApiObjectEncoder).encode()

        request.add_header("Content-Type", "application/json")
        request.add_header("Accept", "application/json")
        request.add_header("Authorization", "Bearer " + self._get_access_token())

        try:
            response = urllib.request.urlopen(request)
            return WaApiClient._parse_response(response)
        except urllib.error.HTTPError as httpErr:
            if httpErr.code == 400:
                raise ApiException(httpErr.read())
            else:
                raise

    def _get_access_token(self):
        expires_at = self._token.retrieved_at + datetime.timedelta(seconds=self._token.expires_in - 100)
        if datetime.datetime.utcnow() > expires_at:
            self._refresh_auth_token()
        return self._token.access_token

    def _refresh_auth_token(self):
        data = {
            "grant_type": "refresh_token",
            "refresh_token": self._token.refresh_token
        }
        encoded_data = urllib.parse.urlencode(data).encode()
        request = urllib.request.Request(self.auth_endpoint, encoded_data, method="POST")
        request.add_header("ContentType", "application/x-www-form-urlencoded")
        auth_header = base64.standard_b64encode((self.client_id + ':' + self.client_secret).encode()).decode()
        request.add_header("Authorization", 'Basic ' + auth_header)
        response = urllib.request.urlopen(request)
        self._token = WaApiClient._parse_response(response)
        self._token.retrieved_at = datetime.datetime.now()

    @staticmethod
    def _parse_response(http_response):
        decoded = json.loads(http_response.read().decode())
        if isinstance(decoded, list):
            result = []
            for item in decoded:
                result.append(ApiObject(item))
            return result
        elif isinstance(decoded, dict):
            return ApiObject(decoded)
        else:
            return None


class ApiException(Exception):
    def __init__(self, value):
        self.value = value

    def __str__(self):
        return repr(self.value)


class ApiObject(object):
    """Represent any api call input or output object"""

    def __init__(self, state):
        self.__dict__ = state
        for key, value in vars(self).items():
            if isinstance(value, dict):
                self.__dict__[key] = ApiObject(value)
            elif isinstance(value, list):
                new_list = []
                for list_item in value:
                    if isinstance(list_item, dict):
                        new_list.append(ApiObject(list_item))
                    else:
                        new_list.append(list_item)
                self.__dict__[key] = new_list

    def __str__(self):
        return json.dumps(self.__dict__)

    def __repr__(self):
        return json.dumps(self.__dict__)


class _ApiObjectEncoder(json.JSONEncoder):
    def default(self, obj):
        if isinstance(obj, ApiObject):
            return obj.__dict__
        # Let the base class default method raise the TypeError
        return json.JSONEncoder.default(self, obj)
