__author__ = 'dsmirnov@wildapricot.com'

import WaApi
import urllib.parse
import json
import logging
import config

API_KEY = config.API_KEY

# create logger with 'spam_application'
# set up logging to file - see previous section for more details
logging.basicConfig(level=logging.INFO,
                             format='%(asctime)s %(levelname)-8s %(message)s',
                             datefmt='%m-%d %H:%M'
                    )


def get_all_active_members():
    # Filter query is for active members only
    # Ids are: 1 - Active
    #          2 - Lapsed
    #          20- PendingNew
    #          3 - PendingRenewal
    params = {'$filter': "'Membership status.Label' eq 'Active'",
              '$async': 'false'}
    request_url = contactsUrl + '?' + urllib.parse.urlencode(params)
    logging.debug(request_url)
    return api.execute_request(request_url).Contacts


def print_contact_info(contact):
    print('Contact details for ' + contact.DisplayName + ', ' + contact.Email)
    print('Main info:')
    print('\tID:' + str(contact.Id))
    print('\tFirst name:' + contact.FirstName)
    print('\tLast name:' + contact.LastName)
    print('\tEmail:' + contact.Email)
    print('\tAll contact fields:')
    for field in contact.FieldValues:
        if field.Value is not None:
            print('\t\t' + field.FieldName + ':' + repr(field.Value))


def create_contact(email, name):
    data = {
        'Email': email,
        'FirstName': name}
    return api.execute_request(contactsUrl, api_request_object=data, method='POST')


def archive_contact(contact_id):
    data = {
        'Id': contact_id,
        'FieldValues': [
            {
                'FieldName': 'Archived',
                'Value': 'true'}]
    }
    return api.execute_request(contactsUrl + str(contact_id), api_request_object=data, method='PUT')

# How to obtain application credentials: https://help.wildapricot.com/display/DOC/API+V2+authentication#APIV2authentication-Authorizingyourapplication
api = WaApi.WaApiClient("CLIENT_ID", "CLIENT_SECRET")
logging.debug(API_KEY)
api.authenticate_with_apikey(API_KEY, scope='auto')
accounts = api.execute_request("/v2/accounts")
if len(accounts) > 0:
    for account in accounts:
        logging.info(account.PrimaryDomainName)

contactsUrl = next(res for res in account.Resources if res.Name == 'Contacts').Url

# Current query returns only the active members
contacts = get_all_active_members()

logging.info('Total number of Active members: {}'.format(len(contacts)))

if logging.getLogger().getEffectiveLevel() is logging.DEBUG:
    for contact in contacts:
        print_contact_info(contact)

# create new contact
#new_copntact = create_contact('some_email1@invaliddomain.org', 'John Doe')
#print_contact_info(new_copntact)

# finally archive it
#archived_contact = archive_contact(new_copntact.Id)
#print_contact_info(archived_contact)