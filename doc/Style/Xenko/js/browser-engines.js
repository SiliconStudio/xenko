/*
 * @author    RocketTheme http://www.rockettheme.com
 * @copyright Copyright (C) 2007 - 2014 RocketTheme, LLC
 * @license   http://www.gnu.org/licenses/gpl-2.0.html GNU/GPLv2 only
 */
(function(){var h=this.document;var e=h.window=this;var a=navigator.userAgent.toLowerCase(),b=navigator.platform.toLowerCase(),f=a.match(/(opera|ie|trident|firefox|chrome|version)[\s\/:]([\w\d\.]+)?.*?(safari|version[\s\/:]([\w\d\.]+)|rv:(\d.?)|$)/)||[null,"unknown",0],d=(f[1]=="ie"||f[1]=="trident")&&h.documentMode;
var i=this.Browser={extend:Function.prototype.extend,name:(f[1]=="version")?f[3]:(f[1]=="trident"?"ie":f[1]),version:d||parseFloat((f[1]=="opera"&&f[4])?f[4]:((f[1]=="trident"&&f[5])?f[5]:f[2])),Platform:{name:a.match(/ip(?:ad|od|hone)/)?"ios":(a.match(/(?:webos|android)/)||b.match(/mac|win|linux/)||["other"])[0]},Features:{xpath:!!(h.evaluate),air:!!(e.runtime),query:!!(h.querySelector),json:!!(e.JSON)},Plugins:{}};
i[i.name]=true;i[i.name+parseInt(i.version,10)]=true;i.Platform[i.Platform.name]=true;i.Request=(function(){var l=function(){return new XMLHttpRequest();
};var k=function(){return new ActiveXObject("MSXML2.XMLHTTP");};var j=function(){return new ActiveXObject("Microsoft.XMLHTTP");};return Function.attempt(function(){l();
return l;},function(){k();return k;},function(){j();return j;});})();i.Features.xhr=!!(i.Request);var g=(Function.attempt(function(){return navigator.plugins["Shockwave Flash"].description;
},function(){return new ActiveXObject("ShockwaveFlash.ShockwaveFlash").GetVariable("$version");})||"0 r0").match(/\d+/g);i.Plugins.Flash={version:Number(g[0]||"0."+g[1])||0,build:Number(g[2])||0};
i.exec=function(k){if(!k){return k;}if(e.execScript){e.execScript(k);}else{var j=h.createElement("script");j.setAttribute("type","text/javascript");j.text=k;
h.head.appendChild(j);h.head.removeChild(j);}return k;};if(i.Platform.ios){i.Platform.ipod=true;}i.Engine={};var c=function(k,j){i.Engine.name=k;i.Engine[k+j]=true;
i.Engine.version=j;};if(i.ie){i.Engine.trident=true;switch(i.version){case 6:c("trident",4);break;case 7:c("trident",5);break;case 8:c("trident",6);}}if(i.firefox){i.Engine.gecko=true;
if(i.version>=3){c("gecko",19);}else{c("gecko",18);}}if(i.safari||i.chrome){i.Engine.webkit=true;switch(i.version){case 2:c("webkit",419);break;case 3:c("webkit",420);
break;case 4:c("webkit",525);}}if(i.opera){i.Engine.presto=true;if(i.version>=9.6){c("presto",960);}else{if(i.version>=9.5){c("presto",950);}else{c("presto",925);
}}}if(i.name=="unknown"){switch((a.match(/(?:webkit|khtml|gecko)/)||[])[0]){case"webkit":case"khtml":i.Engine.webkit=true;break;case"gecko":i.Engine.gecko=true;
}}this.$exec=i.exec;})();