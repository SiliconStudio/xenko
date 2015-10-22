/*
 * @version   $Id: sidemenu.js 14307 2013-10-08 15:44:26Z djamil $
 * @author    RocketTheme http://www.rockettheme.com
 * @copyright Copyright (C) 2007 - 2013 RocketTheme, LLC
 * @license   http://www.gnu.org/licenses/gpl-2.0.html GNU/GPLv2 only
 */
((function(){var b=(function(){return !!("ontouchstart" in window);
})(),a=navigator.userAgent.match(/iPad/i)!=null;var c=this.SideMenu=new Class({initialize:function(){this.build();this.mediaQuery(RokMediaQueries.getQuery());
this.attachEvents();this.mediaQuery(RokMediaQueries.getQuery());},build:function(){if(this.toggler){return this.toggler;}this.toggler=new Element("div.gf-menu-toggle").inject(document.body);
this.container=document.getElement(".gf-menu-device-container");this.wrapper=new Element("div.gf-menu-device-container-wrapper").inject(this.container);
this.container=new Element("div.gf-menu-device-wrapper-sidemenu").wraps(this.container);this.menu=document.getElement(".gf-menu");this.originalPosition=this.menu.getParent();
this.open=false;(3).times(function(){new Element("span.icon-bar").inject(this.toggler);},this);this.container.inject(document.body);return this.toggler;
},attachEvents:function(){var e=this.toggler.retrieve("roknavmenu:click",function(f){f.preventDefault().stopPropagation();this.toggle.call(this,f,this.toggler);
}.bind(this));this.toggler.addEvent("click",e);this.touchEvents={click:function(f){f.preventDefault();},touchstart:function(){this.store("touched",true);
},touchcancel:function(f){f.preventDefault();this.store("touched",false);},touchmove:function(){this.store("touched",false);},touchend:function(f){console.log(this);
if(!this.retrieve("touched",false)){f.preventDefault();}else{location.href=this.get("href");}}};try{RokMediaQueries.on("(max-width: 767px)",this.mediaQuery.bind(this));
RokMediaQueries.on("(min-width: 768px)",this.mediaQuery.bind(this));}catch(d){if(typeof console!="undefined"){console.error('Error [Responsive Menu] while trying to add a RokMediaQuery "match" event',d);
}}},attachTouchEvents:function(){if(b&&!a){$$(".responsive-type-panel .item").removeEvents().forEach(function(d){Object.forEach(this.touchEvents,function(f,e){d.addEventListener(e,f,false);
});},this);}},detachTouchEvents:function(){if(b&&!a){$$(".responsive-type-panel .item").forEach(function(d){Object.forEach(this.touchEvents,function(f,e){d.removeEventListener(e,f,false);
});},this);}},toggle:function(d,e){this.container[!this.open?"addClass":"removeClass"]("gf-sidemenu-size-left");document.body[!this.open?"addClass":"removeClass"]("gf-sidemenu-size-marginleft");
e[!this.open?"addClass":"removeClass"]("active");this.open=!this.open;},mediaQuery:function(e){var f=this.menu,d=this.wrapper;if(!f&&!d){return;}if(e!="(min-width: 768px)"&&e!="(max-width: 767px)"){return;
}if(e=="(min-width: 768px)"){f.inject(this.originalPosition);this.detachTouchEvents();this.toggler.setStyle("display","none");}else{f.inject(d);this.attachTouchEvents();
this.toggler.setStyle("display","block");}this.toggler.removeClass("active");}});window.addEvent("domready",function(){this.RokNavMenu=new c();});})());
