// Modify request headers here
chrome.webRequest.onBeforeSendHeaders.addListener((details) => {
	chrome.tabs.sendMessage(details.tabId, {type:"request", details:details});

	details.requestHeaders.push({name:"X-WebDiff",value:"1"});
	return {requestHeaders: details.requestHeaders};
}, {urls: ["*://*/*"]}, ["requestHeaders", "blocking"]);

// Modify response headers here
chrome.webRequest.onHeadersReceived.addListener((details) => {
	chrome.tabs.sendMessage(details.tabId, {type:"response", details:details});

	if(details.type === "main_frame") {
		chrome.tabs.executeScript(details.tabId, {code:"sessionStorage.setItem('webdiff.http', JSON.stringify(" + JSON.stringify(details) + "));"});
	}
}, {urls: ["*://*/*"]}, ["responseHeaders", "blocking"]);
