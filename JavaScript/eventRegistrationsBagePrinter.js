// this script requires jQuery


function WApublicApi(clientId)
{
   this.clientId = clientId;
   this.apiRequest = function (apiUrl, onSuccess, onError) {
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

   this._onInitSucceed = function (data, textStatus, jqXhr) {
       this.accountId = data[0].Id;
       alert(this);
       alert(this.accountId);
       this.apiUrls = {
           me: function () { return '/sys/api/v2/accounts/' + this.accountId + '/contacts/me'; },
           contacts: function () { return '/sys/api/v2/accounts/' + this.accountId + '/contacts' },
           events: function () { return '/sys/api/v2/accounts/' + this.accountId + '/events' },
           registrations: function () { return '/sys/api/v2/accounts/' + this.accountId + '/events' },
           contactFields: '/sys/api/v2/accounts/' + this.accountId + '/contactFields',
           invoices: '/sys/api/v2/accounts/' + this.accountId + '/invoices',
           payments: '/sys/api/v2/accounts/' + this.accountId + '/payments',
           tenders: '/sys/api/v2/accounts/' + this.accountId + '/tenders'
       };
   };
   

    this.init = function () {
        return this.apiRequest("/sys/api/v2/accounts", this._onInitSucceed,
            function (jqXHR, textStatus, errorThrown) {
                throw { status: textStatus, internalError: errorThrown };
            });
   };
}

function checkClientId() {
    if (clientId) return;

    alert("clientId variable is not declared");
}
checkClientId();

var api = new WApublicApi(clientId);
$.when(api.init()).done(function () {
    alert(api.accountId);
    //api.apiRequest(api.apiUrls.me, function (data, textStatus, jqXhr) {
    //    alert("Hello " + data.FirstName + " " + data.LastName + " !<br>Spirits say that your ID is '" + data.Id + "' and your email is '" + data.Email + "'.");
    //});
});



/*
   function (data, textStatus, jqXhr) {    
        $("#api_result").html("Hello " + data.FirstName + " " + data.LastName +" !<br>Spirits say that your ID is '" + data.Id + "' and your email is '" + data.Email + "'.");
          
   $('#name').html(data.FirstName+" "+data.LastName);
$('#status').html(data.FirstName);             
      },
*/

/*
   function (jqXHR, textStatus, errorThrown) {
        var loginUrl = "/sys/login?ReturnUrl=" + encodeURIComponent('/JS-powered-page');
        $("#api_result").html("Looks like you are not authenticated. Please <a href='" + loginUrl + "'>log in<\/a> and come back.<br> Error: " +  textStatus + " (" + jqXHR.status + ")  " + errorThrown);}
*/