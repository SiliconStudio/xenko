// Copyright (c) 2010-2013 SharpDoc - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.       
// -------------------------------------------------------------------------------
// SplitPane handling Toc and Conent
// -------------------------------------------------------------------------------

// splitPaneId: id to the splitPane container
// splitPaneToggleId: id to the splitPane toggle collapse-expand button
// splitPaneResizerId: id to the resizer grip

// If a search happened, we can't use AJAX anymore to make results disappear. As a result, hightLightTopic() will open new URL as external
searchMode = false;

function supports_local_storage() {
    try {
        return 'localStorage' in window && window['localStorage'] !== null && window.localStorage['getItem'] !== null;
    } catch (e) {
        return false;
    }
}

function autoResize(id) {
    if (document.getElementById) {
        var newheight = "100%";
        if (Browser.firefox || Browser.ie) {
            newheight = (document.getElementById(id).contentWindow.document.body.scrollHeight) + "px";
        }
        document.getElementById(id).height = newheight;
    }
}

function loadContent(rootTopic, extension) {
    var data = window.location.search;
    var url = rootTopic + extension;
    var topicToHighlight = rootTopic;

    if(data != null && data !="")
    {
        var pattern = /page=([^&]+)/;
        var page = pattern.exec(data);
        if(page != null)
        {
            url = page[1].replace(/%/, '%25') + extension;
            topicToHighlight = page[1];
        }
    }
    
    if (window.location.hash)
        topicToHighlight += window.location.hash;
    
    $("mainFrame").setAttribute("src", url);
    hightLightTopic(topicToHighlight, true);
}

/*
  http://webfreak.no/wp/2007/09/05/get-for-mootools-a-way-to-read-get-variables-with-javascript-in-mootools/
Function: $get
	This function provides access to the "get" variable scope + the element anchor

Version: 1.3

Arguments:
	key - string; optional; the parameter key to search for in the url's query string (can also be "#" for the element anchor)
	url - url; optional; the url to check for "key" in, location.href is default

Example:
	>$get("foo","http://example.com/?foo=bar"); //returns "bar"
	>$get("foo"); //returns the value of the "foo" variable if it's present in the current url(location.href)
	>$get("#","http://example.com/#moo"); //returns "moo"
	>$get("#"); //returns the element anchor if any, but from the current url (location.href)
	>$get(,"http://example.com/?foo=bar&bar=foo"); //returns {foo:'bar',bar:'foo'}
	>$get(,"http://example.com/?foo=bar&bar=foo#moo"); //returns {foo:'bar',bar:'foo',hash:'moo'}
	>$get(); //returns same as above, but from the current url (location.href)
	>$get("?"); //returns the query string (without ? and element anchor) from the current url (location.href)

Returns:
	Returns the value of the variable form the provided key, or an object with the current GET variables plus the element anchor (if any)
	Returns "" if the variable is not present in the given query string

Credits:
		Regex from [url=http://www.netlobo.com/url_query_string_javascript.html]http://www.netlobo.com/url_query_string_javascript.html[/url]
		Function by Jens Anders Bakke, webfreak.no
*/
function $get(key, url) {
    if (arguments.length < 2) url = location.href;
    if (arguments.length > 0 && key != "") {
        if (key == "#") {
            var regex = new RegExp("[#]([^$]*)");
        } else if (key == "?") {
            var regex = new RegExp("[?]([^#$]*)");
        } else {
            var regex = new RegExp("[?&]" + key + "=([^&#]*)");
        }
        var results = regex.exec(url);
        return (results == null) ? "" : results[1];
    } else {
        url = url.split("?");
        var results = {};
        if (url.length > 1) {
            url = url[1].split("#");
            if (url.length > 1) results["hash"] = url[1];
            url[0].split("&").each(function (item, index) {
                item = item.split("=");
                results[item[0]] = item[1];
            });
        }
        return results;
    }
}


function InstallCodeTabs() {
    var groupTabs = $$('.grouptab');
    groupTabs.each(function (groupTab, groupIndex) {
        var tabs = groupTab.getChildren('.tabs li.tab');
        var content = groupTab.getChildren('.tabcontent');
        tabs.each(function (tab, index) {
            tab.addEvent('click', function () {
                tabs.removeClass('selected');
                content.removeClass('selected');
                tabs[index].addClass('selected');
                content[index].addClass('selected');
            });
        });
    });
}

function SplitPane(splitPaneId, splitPaneToggleId, splitPaneResizerId) {

    // Define column elemnts
    var paneLeft = $(splitPaneId);
    var paneRight = $("main_content");
    var splitPaneResizer = $(splitPaneResizerId);
    var splitPaneToggle = $(splitPaneToggleId);
    var iframe = $('mainFrame');

    var paneLeftMinWidth = 100;
    var paneLeftOriginalWidth = paneLeft.getWidth();
    splitPaneResizer.setStyle('left', paneLeftOriginalWidth + 3);
    paneRight.setStyle('left', paneLeftOriginalWidth);

    var splitPaneOriginalLeft = splitPaneResizer.getLeft();
    var paneRightOriginalLeft = paneRight.getLeft();

    // Use localstorage to store toggle state
    if (supports_local_storage()) {
        if (localStorage.getItem('sharpdoc-resize')) {
            var value = parseInt(localStorage.getItem('sharpdoc-resize'));
            if (value == 0) {
                splitPaneToggle.set('class', 'expand');
                paneLeft.setStyle('display', 'none');
            }
            paneLeft.setStyle('width', value);
            paneLeftOriginalWidth = paneLeft.getWidth();
            splitPaneResizer.setStyle('left', paneLeftOriginalWidth + 3);
            paneRight.setStyle('left', paneLeftOriginalWidth);
        }
    }

    //  Snap size for resizer
    var resizerSnap = 5;

    // Make the left cell resizable
    paneLeft.makeResizable({
        handle: $(splitPaneResizerId),
        grid: resizerSnap,
        modifiers: { x: 'width', y: false },
        limit: { x: [paneLeftMinWidth, null] },
        onStart: function (el) {
            // Disable pointer events on iframe while dragging
            // otherwise we can't drag over the iframe
            iframe.setStyle('pointer-events', 'none');
        },
        onComplete: function(el) {
            // Enable back pointer events on iframe after dragging
            // is complete
            iframe.setStyle('pointer-events', 'auto');
        },
        onDrag: function (el) {
            splitPaneResizer.setStyle('left', el.getWidth() + 3);
            paneRight.setStyle('left', el.getWidth());
            if (supports_local_storage()) {
                localStorage.setItem('sharpdoc-resize', el.getWidth());
            }
        },
    });

    var topTitle = $$('h1.content-title');

    var expandCollapseFunction = function (event) {
        if (paneLeft.getWidth() < paneLeftMinWidth) {
            splitPaneToggle.set('class', 'collapse');
            paneLeft.setStyle('display', 'block');
            
            // Morph the following values
            paneLeft.morph({'width': paneLeftOriginalWidth,'opacity': '1'});
            splitPaneResizer.morph({ 'left': splitPaneOriginalLeft + 12 });  // not sure why we need to add 12 to have a correct display
            paneRight.morph({ 'left': paneRightOriginalLeft + 12});

            if (supports_local_storage()) {
                localStorage.removeItem('sharpdoc-resize');
            }
        } else {
            splitPaneToggle.set('class', 'expand');
            paneLeft.set('morph', { link: 'chain' }).morph({ 'width': '1', 'opacity': '0' }).morph({ 'display': 'none' });
            splitPaneResizer.morph({ 'left': '1' });
            paneRight.morph({ 'left': '1' });

            if (supports_local_storage()) {
                localStorage.setItem('sharpdoc-resize', 0);
            }
        }
    };

    // Handle toggle button collapse-expand events
    splitPaneToggle.addEvent('click', expandCollapseFunction);
    if (topTitle.length > 0) {
        topTitle[0].addEvent('click', expandCollapseFunction);
    }
}

function openToc(nodeId) {
    //alert("open");
    var node = $_(nodeId + "_toc");

    if (!node.hasClass('opened')) {
        var subNodes = $_(nodeId + "_SubTopics");
        if (subNodes != undefined)
            subNodes.set('class', 'visible');

        node.removeClass('closed');
        node.addClass('opened');
    }

    var parent = node.getParent().id;
    var pattern = /^(.+)_SubTopics$/;
    var parentId = pattern.exec(parent);
    if (parentId != null)
        openToc(parentId[1]);
}

function closeToc(nodeId) {
    //alert("close");
    var node = $_(nodeId + "_toc");

    // The highlighten topic could not be closed
    if (!node.hasClass('highlighting')) {
        var subNodes = $_(nodeId + "_SubTopics");
        if (subNodes != undefined)
            subNodes.set('class', 'hidden');

        node.removeClass('opened');
        node.addClass('closed');
    }
    else {
        node.removeClass('highlighting');
    }
}

function toggleToc(nodeId) {
    //alert("toggle");
    var node = $_(nodeId + "_toc");

    if (node.hasClass('closed'))
        openToc(nodeId);
    else
        closeToc(nodeId);

}

function hightLightTopic(topicId, keepState, event) {
    //alert("highlight");
    
    var fullTopicId = topicId; // including anchor
    var anchorIndex = topicId.indexOf('#');
    var anchor = anchorIndex != -1 ? topicId.substring(anchorIndex) : null;
    topicId = anchorIndex != -1 ? topicId.substring(0, anchorIndex) : topicId;
    
    if (topicId == "external" || searchMode)
        return;
    
    var oldHighlight = $$_('.highlight');
    oldHighlight.each(function (old, oldId) {
        old.removeClass('highlight');
    });

    if (event) {
        event.stopImmediatePropagation();
        event.preventDefault();
    }

    if (keepState !== true) {
        $('mainFrame').set('load', {evalScripts: false});
        $('mainFrame').load("html/" + escape(topicId) + ".htm");
        $('mainFrame').get('load').onSuccess = function(e) {
          InstallCodeTabs();
          // Syntax Highlighter should reprocess new code blocks
          SyntaxHighlighter.highlight();
    
          // Scroll to top
          $('main_content').scrollTo(0);
    
          // If there was an anchor, keep it
          if (anchor)
          {
              var baseUrl = window.location.href.split('#')[0];
              window.location.replace( baseUrl + anchor );
          }
        };
    }
    
    var newHightlight = $_(topicId + '_toc');
    if (newHightlight == null)
        return;

    newHightlight.addClass('highlight');
    newHightlight.addClass('highlighting');
    openToc(topicId);

    if (!keepState)
        history.pushState(fullTopicId, "", "?page=" + fullTopicId);
    else
        history.replaceState(fullTopicId, "", "?page=" + fullTopicId);
}

window.onpopstate = function(e) {
  if (e.state) {
    hightLightTopic(e.state, true);
  }
}

function $_(id) {
    var element = $(id);
    if (element == undefined)
        element = window.parent.$(id);
    return element;
}

function $$_(id) {
    var elements = $$(id);
    if (elements == undefined || elements.length == 0)
        elements = window.parent.$$(id);
    return elements;
}