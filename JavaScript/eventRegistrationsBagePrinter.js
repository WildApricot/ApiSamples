// this script requires jQuery


function WApublicApi(clientId) {
    /**
    * WApublicApi is a thin wrapper around $.ajax, which helps to access wild apricot public api.
    * @clientId {String} is a valid client application ID. See https://help.wildapricot.com/display/DOC/Authenticating+API+access+from+a+Wild+Apricot+site+page for details.
    */
    this._initialized = false;
    this.clientId = clientId;
    this.apiRequest = function (apiUrl, onSuccess, onError) {
        if (!this._initialized) {
            throw "api client is not initialized yet. Please call init() before usaing api. Ex: $.when(api.init()).done(function(){  api.apiRequest(...); });";
        }
        return $.ajax({
            url: apiUrl,
            type: "GET",
            dataType: "json",
            cache: false,
            headers: { "clientId": this.clientId },
            success: onSuccess,
            error: onError,
            context: this
        });
    };

    this.apiUrls = {
        accountId: -1,
        account: function () { return '/sys/api/v2/accounts/' + this.accountId },
        me: function () {
            /**
            * contacts/me api call. Returns basic information on current logged in user
            */
            return this.account() + '/contacts/me'
        },
        contact: function (contactId) {
            /**
            * specific contact api call  (https://help.wildapricot.com/display/DOC/Contacts+API+V2+call#ContactsAPIV2call-Retrievinginformationforaspecificcontact)
            * @contactId {Number}
            */
            
            return this.account() + '/contacts/' + contactId
        },
        contacts: function (simpleQuery, params) {
            /**
            * contacts api call with optional params (http://help.wildapricot.com/display/DOC/Contacts+API+V2+call)
            * @simpleQuery {String} filter contacts by email, display name, first name, last name etc
            * @params {Object} other params supported by contacts api: $async, $top, $skip, $filter
            */
            
            if (params == null) params = { };
            
            if (simpleQuery) params.simpleQuery = simpleQuery;
            if (typeof (params.$async) == 'undefined' || params.$async == null) params.$async = false;

            var result = this.account() + '/contacts';

            return result + $.param(params);
        },
        event: function (eventId) {
            /**
            * specific event api call  (https://help.wildapricot.com/display/DOC/Events+API+V2+call#EventsAPIV2call-Retrievinginformationforaparticularevent)
            * @eventId {Number}
            */
            return this.account() + '/events/' + eventId
        },
        events: function (simpleQuery, params) {
            /**
             * events api call with optional params (http://help.wildapricot.com/display/DOC/Events+API+V2+call)
             * @simpleQuery {String} filter events by title, description, location etc
             * @params {Object} other params supported by events api: $top, $skip, $filter, includeEventDetails
             */

            if (params == null) params = {};
            if (simpleQuery) params.simpleQuery = simpleQuery;

            var result = this.account() + '/events';

            return result + $.param(params);
        },
        registrations: function (params) {
            /**
             * event registrationss api call with optional params (https://help.wildapricot.com/display/DOC/EventRegistrations+API+V2+call)
             * @params {Object} params supported by event registrations api: contactId, eventId, $filter
             *                  at least one of these parameters should be passed
             */
            return this.account() + '/eventregistrations' + $.param(params)
        },
        contactFields: function (params) { return this.account() + '/contactFields' + $.param(params) },
        invoice: function (invoiceId) {
            /**
             * particular invoice api call (https://help.wildapricot.com/display/DOC/Invoices+API+V2+call#InvoicesAPIV2call-Retrievinginformationforaparticularinvoice)
             * @invoiceId {Number}
             */
            return this.account() + '/invoices' + $.param(params)
        },
        invoices: function (params) {
            /**
             * invoices api call with optional params (https://help.wildapricot.com/display/DOC/Invoices+API+V2+call)
             * @params {Object} params supported by invoices api: contactId, eventId
             *                  at least one of these parameters should be passed
             */
            return this.account() + '/invoices' + $.param(params)
        },
        payments: function (params) {
            /**
            * payments api call with optional params (https://help.wildapricot.com/display/DOC/Payments+API+V2+call)
            * @params {Object} params supported by payments api: contactId, eventId
            *                  at least one of these parameters should be passed
            */
            return this.account() + '/payments' + $.param(params)
        },
        tenders: function (params) { return this.account() + '/tenders' + $.param(params) }
    };

    this._onInitSucceed = function (data, textStatus, jqXhr) {
        this.accountId = data[0].Id;
        this.apiUrls.accountId = this.accountId;
        this._initialized = true;
    };

    this.init = function () {
        return $.ajax({
            url: "/sys/api/v2/accounts",
            type: "GET",
            dataType: "json",
            cache: false,
            headers: { "clientId": this.clientId },
            success: this._onInitSucceed,
            error: function (jqXHR, textStatus, errorThrown) {
                throw { status: textStatus, internalError: errorThrown };
            },
            context: this
        });
    };
}

function checkClientId() {
    if (clientId) return;

    alert("clientId variable is not declared");
}
checkClientId();

var api = new WApublicApi(clientId);
$.when(api.init())
 .done(function () {
     api.apiRequest(api.apiUrls.me(), function (data, textStatus, jqXhr) {
         alert("Hello " + data.FirstName + " " + data.LastName + " !<br>Spirits say that your ID is '" + data.Id + "' and your email is '" + data.Email + "'.");
     });
 });