// This file is generated dynamically per-request and cannot be used directly.
// To use WURFL.js, please see the documentation at https://web.wurfl.io/#wurfl-js
//
// Copyright 2023 - ScientiaMobile, Inc., Reston, VA
// WURFL Device Detection
// Terms of service:
// https://www.scientiamobile.com/terms-of-service-wurfl-js-lite/
//
// For improved iPhone/iPad detection, upgrade to WURFL.js Business Edition:
// https://www.scientiamobile.com/products/wurfl-js/

var WURFL = { complete_device_name: "Microsoft Edge", form_factor: "Desktop", is_mobile: !1 }, newEvent, wurfl_debug = !1; "Promise" in window && (window.WURFLPromises = { init: new Promise(function (e) { e({ WURFL }) }), complete: new Promise(function (e) { e({ WURFL }) }) }), "CustomEvent" in window ? (document.dispatchEvent(new CustomEvent("WurflJSInitComplete", { bubbles: !0, detail: { WURFL } })), document.dispatchEvent(new CustomEvent("WurflJSDetectionComplete", { bubbles: !0, detail: { WURFL } }))) : (newEvent = document.createEvent("CustomEvent"), newEvent.initCustomEvent("WurflJSInitComplete", !0, !0, { WURFL }), document.dispatchEvent(newEvent), newEvent = document.createEvent("CustomEvent"), newEvent.initCustomEvent("WurflJSDetectionComplete", !0, !0, { WURFL }), document.dispatchEvent(newEvent))