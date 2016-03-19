// this script requires jQuery and waPublicApi
// this is a sample js app for printing event bages

var api = new WApublicApi(clientId);
$.when(api.init()).done(function () {


    // create event selection UI and fill it with events
    var eventSelector = $("<select id='eventSelector'></select>");
    
    api.apiRequest({
        apiUrl: api.apiUrls.events({$filter:"isUpcoming eq true"}),
        success: function(data, textStatus, jqXhr) {
            eventSelector.find("option").remove();
            $.each(data, function(key, value) {   
                eventSelector.append($('<option>', { value : value.Id }).text(value.Name)); 
            });
        }
    });
    
    // create badge constructor: size, background image, content, page breaks (page-break-after:always or page-break-inside:avoid) 
    var badgeBuilder = $("<div id='badgeBuilder'></div>");
    
    
    
    
    // create "print" button
    
    // load event registrations
    
    // build custom html and open print dialog
    
    api.apiRequest( {
      apiUrl: api.apiUrls.me(),
      success: function (data, textStatus, jqXhr) {
         alert("Hello " + data.FirstName + " " + data.LastName + " !<br>Spirits say that your ID is '" + data.Id + "' and your email is '" + data.Email + "'."); } });
 });
