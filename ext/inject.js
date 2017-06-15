chrome.runtime.onMessage.addListener(function(message, sender, sendResponse) {
	//console.log(JSON.stringify(message));
});

console.log('WebDiff extension injected');
