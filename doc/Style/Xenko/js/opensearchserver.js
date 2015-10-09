/**
 * License Agreement for OpenSearchServer
 * 
 * Copyright (C) 2012 Emmanuel Keller / Jaeksoft
 * 
 * http://www.open-search-server.com
 * 
 * This file is part of OpenSearchServer.
 * 
 * OpenSearchServer is free software: you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published by the Free
 * Software Foundation, either version 3 of the License, or (at your option) any
 * later version.
 * 
 * OpenSearchServer is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more
 * details.
 * 
 * You should have received a copy of the GNU General Public License along with
 * OpenSearchServer. If not, see <http://www.gnu.org/licenses/>.
 */

if (typeof (OpenSearchServer) == "undefined")
	OpenSearchServer = {};

OpenSearchServer.getXmlHttpRequestObject = function() {
	if (window.XMLHttpRequest) {
		return new XMLHttpRequest();
	} else if (window.ActiveXObject) {
		return new ActiveXObject("Microsoft.XMLHTTP");
	} else {
		return null;
	}
};

OpenSearchServer.xmlHttp = OpenSearchServer.getXmlHttpRequestObject();

OpenSearchServer.setAutocomplete = function(divautocompid, value) {
	var ac = document.getElementById(divautocompid);
	ac.innerHTML = value;
	return ac;
};

OpenSearchServer.getselectedautocompletediv = function(divautocompid, n) {
	return document.getElementById(divautocompid + 'item' + n);
};

OpenSearchServer.autosuggest = function(event, urlwithparam, formid, textid,
		divautocompid) {
	var oldSelected = OpenSearchServer.getSelected(divautocompid);
	var newSelected = 0;
	var keynum = 0;
	if (window.event) { // IE
		keynum = event.keyCode;
	} else if (event.which) { // Netscape/Firefox/Opera
		keynum = event.which;
	}
	if (keynum == 38 || keynum == 40) {
		if (keynum == 38) {
			if (oldSelected > 0) {
				newSelected = oldSelected - 1;
			}
		} else if (keynum == 40) {
			if (OpenSearchServer.getselectedautocompletediv(divautocompid,
					oldSelected + 1) != null) {
				newSelected = oldSelected + 1;
			}
		}
		if (newSelected > 0) {
			var dv = OpenSearchServer.getselectedautocompletediv(divautocompid,
					newSelected);
			OpenSearchServer.autocompleteLinkOver(divautocompid, newSelected,
					oldSelected);
			OpenSearchServer.setKeywords(textid, dv.innerHTML);
		}
		return false;
	}

	if (OpenSearchServer.xmlHttp.readyState != 4
			&& OpenSearchServer.xmlHttp.readyState != 0)
		return;
	var str = document.getElementById(textid).value;
	if (str.length == 0) {
		OpenSearchServer.setAutocomplete(divautocompid, '');
		return;
	}

	OpenSearchServer.xmlHttp.open("GET",
			urlwithparam + encodeURIComponent(str), true);
	OpenSearchServer.xmlHttp.onreadystatechange = function() {
		OpenSearchServer.handleAutocomplete(formid, textid, divautocompid, str);
	};
	OpenSearchServer.xmlHttp.send(null);
	return true;
};

OpenSearchServer.getSelected = function(divautocompid) {
	var i = 1;
	do {
		dv = OpenSearchServer.getselectedautocompletediv(divautocompid, i);
		if (dv == null)
			return 0;
		if (dv.className == 'ossautocomplete_link_over') {
			return i;
		}
		i++;
	} while (dv != null);
	return 0;
};

OpenSearchServer.handleAutocomplete = function(formid, textid, divautocompid,
		keyword) {
	if (OpenSearchServer.xmlHttp.readyState != 4)
		return;
	var ac = OpenSearchServer.setAutocomplete(divautocompid, '');
	var resp = OpenSearchServer.xmlHttp.responseText;
	if (resp == null) {
		return;
	}
	if (resp.length == 0) {
		return;
	}
	var str = resp.split("\n");
	var content = '<div id="' + divautocompid + 'list">';
	for ( var i = 0; i < str.length - 1; i++) {
		var j = i + 1;
		content += '<div id="' + divautocompid + 'item' + j + '" ';
		content += 'onmouseover="javascript:OpenSearchServer.autocompleteLinkOver(\''
				+ divautocompid + '\',' + j + ');" ';
		content += 'onmouseout="javascript:OpenSearchServer.autocompleteLinkOut(\''
				+ divautocompid + '\',' + j + ');" ';
		content += 'onclick="javascript:OpenSearchServer.setKeywords_onClick(\''
				+ formid
				+ '\',\''
				+ textid
				+ '\',this.innerHTML,\''
				+ divautocompid + '\');" ';
		line = '<span class="ossautocomplete_chars">'
				+ str[i].substring(0, keyword.length) + '</span>'
				+ str[i].substring(keyword.length);
		content += 'class="ossautocomplete_link">' + line + '</div>';
	}
	content += '</div>';
	ac.innerHTML = content;
};

OpenSearchServer.autocompleteLinkOver = function(divautocompid, newSelected,
		oldSelected) {
	if (oldSelected == null) {
		oldSelected = OpenSearchServer.getSelected(divautocompid);
	}
	if (oldSelected > 0) {
		OpenSearchServer.autocompleteLinkOut(divautocompid, oldSelected);
	}
	var dv = OpenSearchServer.getselectedautocompletediv(divautocompid,
			newSelected);
	dv.className = 'ossautocomplete_link_over';
};

OpenSearchServer.autocompleteLinkOut = function(divautocompid, oldSelected) {
	var dv = OpenSearchServer.getselectedautocompletediv(divautocompid,
			oldSelected);
	dv.className = 'ossautocomplete_link';
};

OpenSearchServer.setKeywords_onClick = function(formid, textid, value,
		divautocompid) {
    displaySearchResult();
	var dv = document.getElementById(textid);
	if (dv != null) {
		dv.value = value.replace(/(<([^>]+)>)/ig, "");
		dv.focus();
		OpenSearchServer.setAutocomplete(divautocompid, '');
		document.forms[formid].submit();
		return true;
	}
};

OpenSearchServer.setKeywords = function(textid, value) {
	var dv = document.getElementById(textid);
	if (dv != null) {
		dv.value = value.replace(/(<([^>]+)>)/ig, "");
		dv.focus();
	}
};