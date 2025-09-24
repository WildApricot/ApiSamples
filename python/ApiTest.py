__author__ = 'dsmirnov@wildapricot.com'

import WaApi
import urllib.parse
import json

def fetchAllContacts():
    top = 100
    skip = 0
    all_contacts = []

    while True:
        params = {'$top': str(top),
                  '$skip': str(skip),
                  '$async': 'false'}
        request_url = contactsUrl + '?' + urllib.parse.urlencode(params)
        print(request_url)
        resp = api.execute_request(request_url)

        # response may expose Contacts property
        page = resp.Contacts if hasattr(resp, 'Contacts') else resp

        if not page:
            break

        all_contacts.extend(page)

        # if returned less than requested page size, no more records
        if len(page) < top:
            break

        skip += top

    return all_contacts

def get_10_active_members():
    params = {'$filter': 'member eq true',
              '$top': '10',
              '$async': 'false'}
    request_url = contactsUrl + '?' + urllib.parse.urlencode(params)
    print(request_url)
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
api.authenticate_with_contact_credentials("ADMINISTRATOR_USERNAME", "ADMINISTRATOR_PASSWORD")
accounts = api.execute_request("/v2/accounts")
account = accounts[0]

print(account.PrimaryDomainName)

contactsUrl = next(res for res in account.Resources if res.Name == 'Contacts').Url

# get top 10 active members and print their details
contacts = get_10_active_members()
for contact in contacts:
    print_contact_info(contact)

# create new contact
new_copntact = create_contact('some_email1@invaliddomain.org', 'John Doe')
print_contact_info(new_copntact)

# finally archive it
archived_contact = archive_contact(new_copntact.Id)
print_contact_info(archived_contact)

allContacts = fetchAllContacts()
print('Total contacts fetched: ' + str(len(allContacts)))