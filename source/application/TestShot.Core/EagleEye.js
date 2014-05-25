//try to get id if not found get xpath
//Basically a copy of Huxley BigBrother
(function () {
	/* global window */

	'use strict';

	var events = [];
	var specialKeysMap = {
		'37': 'ARROW_LEFT',
		'38': 'ARROW_UP',
		'39': 'ARROW_RIGHT',
		'40': 'ARROW_DOWN',
		'8': 'BACK_SPACE',
		'46': 'DELETE'
	};

	// we treat a click as a mousedown, because they behave the same way
	// except when, say, clicking on a label that causes the checkbox to be
	// checked. In this case, the click event fires twice, which is undesirable

	// slightly related: for a select, chrome triggers mousedown but not click
	// anyways
	window.addEventListener('mouseup', function (e) {
		//Get target element
		var targ;
		if (!e) var e = window.event;
		if (e.target) targ = e.target;
		else if (e.srcElement) targ = e.srcElement;
		if (targ.nodeType == 3) // defeat Safari bug
			targ = targ.parentNode;

		events.push({
			Action: 'Click',
			Timestamp: Date.now(),
			X: e.clientX,
			Y: e.clientY,
			Id: e.id,
			Xpath: createXPathFromElement(e)
		});
	}, true);

	// only `keypress` returns the correct character. `keydown` returns `A` when
	// pressing `a`, etc.
	window.addEventListener('keypress', function (e) {
		// chrome doesn't trigger `keypress` for arrow keys, backspace and delete.
		// ff does. Just handle them apart below in `keydown`
		var code = e.keyCode || e.which;
		// arrow keys
		if (code >= 37 && code <= 40) return;

		// firefox detects backspace and delete on keypress while webkit doesn't
		// we'll delegate to keydown to handle these two keys. We need to treat
		// then as special recorded keys anyways for the selenium replay

		// backspace
		if (code === 8) return;

		// delete Key:
		//   keypress:
		//     ff: keyCode 46 and which 0
		//     chrome: not triggered
		//   keydown:
		//     ff: keyCode 46 and which 46
		//     chrome: keyCode 46 and which 46

		// period Key:
		//   keypress:
		//     ff: keyCode 0 and which 46
		//     chrome: keyCode 46 and which 46
		//   keydown:
		//     ff: keyCode 190 and which 190
		//     chrome: keyCode 190 and which 190

		if (e.keyCode === 46 && e.which === 0) return;

		// the way dom deals with keydown/press/up and which charCode/keyCode/which
		// we receive is really screwed up. If you google "keyCode charCode which"
		// you'll see why the `or` is here
		events.push({
			Action: 'Keypress',
			Timestamp: Date.now(),
			Key: String.fromCharCode(code)
		});
	}, true);

	// arrow keys are not registered by `keypress` in chrome, so handle them here.
	// They also clash with `&`, `%`, `'` and `(`, so map them to a something
	// readable for now. Will take care of processing this and simulating the
	// keypress correctly in playback mode.
	window.addEventListener('keydown', function (e) {
		var code = e.keyCode || e.which;
		if ((code < 37 || code > 40) && (code !== 8 && code !== 46)) return;
		// treat it as a `keypress` for simplicity during playback simulation
		events.push({
			Action: 'Keypress',
			Timestamp: Date.now(),
			Key: specialKeysMap[code]
		});
	}, true);

	window.onscroll = function () {
		events.push({
			Action: 'Scroll',
			Timestamp: Date.now(),
			X: window.scrollX,
			Y: window.scrollY
		});
	};

	// TODO: maybe double click and right click in the future, but Selenium
	// support and manual reproduction are shaky
	window._getTestShotEvents = function () {
		return events;
	};
})();

//http://stackoverflow.com/questions/2661818/javascript-get-xpath-of-a-node
//I refactored this from another example. It will attempt to check or there is for sure a unique id and if so use that case to shorten the expression.

function createXPathFromElement(elm) { 
    var allNodes = document.getElementsByTagName('*'); 
    for (segs = []; elm && elm.nodeType == 1; elm = elm.parentNode) 
{ 
        if (elm.hasAttribute('id')) { 
                var uniqueIdCount = 0; 
                for (var n=0;n < allNodes.length;n++) { 
                    if (allNodes[n].hasAttribute('id') && allNodes[n].id == elm.id) uniqueIdCount++; 
                    if (uniqueIdCount > 1) break; 
}; 
                if ( uniqueIdCount == 1) { 
                    segs.unshift('id("' + elm.getAttribute('id') + '")'); 
                    return segs.join('/'); 
} else { 
                    segs.unshift(elm.localName.toLowerCase() + '[@id="' + elm.getAttribute('id') + '"]'); 
} 
} else if (elm.hasAttribute('class')) { 
            segs.unshift(elm.localName.toLowerCase() + '[@class="' + elm.getAttribute('class') + '"]'); 
} else { 
            for (i = 1, sib = elm.previousSibling; sib; sib = sib.previousSibling) { 
                if (sib.localName == elm.localName)  i++; }; 
                segs.unshift(elm.localName.toLowerCase() + '[' + i + ']'); 
}; 
}; 
    return segs.length ? '/' + segs.join('/') : null; 
}; 

function lookupElementByXPath(path) { 
    var evaluator = new XPathEvaluator(); 
    var result = evaluator.evaluate(path, document.documentElement, null,XPathResult.FIRST_ORDERED_NODE_TYPE, null); 
    return  result.singleNodeValue; 
} 