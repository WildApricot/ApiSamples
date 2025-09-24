
//load contacts for testin
// Set clientId before including this script
var api = new WApublicApi(clientId);
$.when(api.init()).done(function () {
      (function loadAllContacts() {
          var allContacts = [];
          var skip = 0;
          var top = 100; // API limit

          function fetchPage() {
              api.apiRequest({
                  apiUrl: api.apiUrls.contacts({ $async: false, $top: top, $skip: skip }),
                  success: function (data, textStatus, jqXhr) {
                      var page = (data && data.Contacts) ? data.Contacts : [];
                      if (page.length > 0) {
                          allContacts = allContacts.concat(page);
                          // if page was full, there may be more
                          if (page.length === top) {
                              skip += top;
                              fetchPage();
                              return;
                          }
                      }
                      // no more records
                      alert('Total contacts fetched: ' + allContacts.length);
                      // you can use allContacts here for further processing
                  },
                  error: function (jqXhr, textStatus, errorThrown) {
                      alert("Error fetching contacts: " + errorThrown);
                  }
              });
          }

          // start fetching
          fetchPage();
      })();
    });