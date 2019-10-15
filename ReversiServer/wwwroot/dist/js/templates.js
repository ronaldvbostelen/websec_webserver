this["SPA"] = this["SPA"] || {};
this["SPA"]["Template"] = this["SPA"]["Template"] || {};
this["SPA"]["Template"]["board"] = Handlebars.template({"1":function(container,depth0,helpers,partials,data,blockParams,depths) {
    var stack1;

  return ((stack1 = helpers.each.call(depth0 != null ? depth0 : (container.nullContext || {}),depth0,{"name":"each","hash":{},"fn":container.program(2, data, 0, blockParams, depths),"inverse":container.noop,"data":data})) != null ? stack1 : "");
},"2":function(container,depth0,helpers,partials,data,blockParams,depths) {
    var helper, alias1=container.lambda, alias2=container.escapeExpression;

  return "      <div id=\""
    + alias2(alias1((container.data(data, 1) && container.data(data, 1).index), depth0))
    + "_"
    + alias2(((helper = (helper = helpers.index || (data && data.index)) != null ? helper : helpers.helperMissing),(typeof helper === "function" ? helper.call(depth0 != null ? depth0 : (container.nullContext || {}),{"name":"index","hash":{},"data":data}) : helper)))
    + "\" class=\"tile ui-droppable\">"
    + alias2(alias1(depth0, depth0))
    + "</div>\r\n";
},"compiler":[7,">= 4.0.0"],"main":function(container,depth0,helpers,partials,data,blockParams,depths) {
    var stack1;

  return "<div id=\"board\">\r\n"
    + ((stack1 = helpers.each.call(depth0 != null ? depth0 : (container.nullContext || {}),(depth0 != null ? depth0.Board : depth0),{"name":"each","hash":{},"fn":container.program(1, data, 0, blockParams, depths),"inverse":container.noop,"data":data})) != null ? stack1 : "")
    + "</div>\r\n";
},"useData":true,"useDepths":true});
this["SPA"]["Template"]["enemy"] = Handlebars.template({"compiler":[7,">= 4.0.0"],"main":function(container,depth0,helpers,partials,data) {
    var helper;

  return "<div id=\"playingAgainst\">You are playing against: <span>"
    + container.escapeExpression(((helper = (helper = helpers.enemy || (depth0 != null ? depth0.enemy : depth0)) != null ? helper : helpers.helperMissing),(typeof helper === "function" ? helper.call(depth0 != null ? depth0 : (container.nullContext || {}),{"name":"enemy","hash":{},"data":data}) : helper)))
    + "</span></div>\r\n";
},"useData":true});
this["SPA"]["Template"]["player"] = Handlebars.template({"1":function(container,depth0,helpers,partials,data) {
    return "      Black\r\n";
},"3":function(container,depth0,helpers,partials,data) {
    return "      White\r\n";
},"compiler":[7,">= 4.0.0"],"main":function(container,depth0,helpers,partials,data) {
    var stack1, helper, alias1=depth0 != null ? depth0 : (container.nullContext || {});

  return "<div id=\"playing\">Currently playing as: <span>"
    + container.escapeExpression(((helper = (helper = helpers.playerName || (depth0 != null ? depth0.playerName : depth0)) != null ? helper : helpers.helperMissing),(typeof helper === "function" ? helper.call(alias1,{"name":"playerName","hash":{},"data":data}) : helper)))
    + "</span>\r\n  <div>Color:\r\n"
    + ((stack1 = helpers["if"].call(alias1,(depth0 != null ? depth0.enemy : depth0),{"name":"if","hash":{},"fn":container.program(1, data, 0),"inverse":container.program(3, data, 0),"data":data})) != null ? stack1 : "")
    + "  </div>\r\n</div>\r\n";
},"useData":true});
this["SPA"]["Template"]["popup"] = Handlebars.template({"1":function(container,depth0,helpers,partials,data) {
    return "  <div id=\"btnDiv\">\r\n    <button>Decline</button>\r\n    <button>Accept</button>\r\n  </div>\r\n";
},"3":function(container,depth0,helpers,partials,data) {
    return "    <button id=\"newGameBtn\">New Game</button>\r\n";
},"compiler":[7,">= 4.0.0"],"main":function(container,depth0,helpers,partials,data) {
    var stack1, helper, alias1=depth0 != null ? depth0 : (container.nullContext || {}), alias2=helpers.helperMissing, alias3="function", alias4=container.escapeExpression;

  return "<div id=\"popup\" style=\"background-color: "
    + alias4(((helper = (helper = helpers.color || (depth0 != null ? depth0.color : depth0)) != null ? helper : alias2),(typeof helper === alias3 ? helper.call(alias1,{"name":"color","hash":{},"data":data}) : helper)))
    + ";\">\r\n  <div id=\"close\">\r\n    <span></span>\r\n    <span></span>\r\n  </div>\r\n  <h1>"
    + alias4(((helper = (helper = helpers.title || (depth0 != null ? depth0.title : depth0)) != null ? helper : alias2),(typeof helper === alias3 ? helper.call(alias1,{"name":"title","hash":{},"data":data}) : helper)))
    + "</h1>\r\n  <p>"
    + alias4(((helper = (helper = helpers.message || (depth0 != null ? depth0.message : depth0)) != null ? helper : alias2),(typeof helper === alias3 ? helper.call(alias1,{"name":"message","hash":{},"data":data}) : helper)))
    + "</p>\r\n"
    + ((stack1 = helpers["if"].call(alias1,(depth0 != null ? depth0.draw : depth0),{"name":"if","hash":{},"fn":container.program(1, data, 0),"inverse":container.noop,"data":data})) != null ? stack1 : "")
    + ((stack1 = helpers["if"].call(alias1,(depth0 != null ? depth0.close : depth0),{"name":"if","hash":{},"fn":container.program(3, data, 0),"inverse":container.noop,"data":data})) != null ? stack1 : "")
    + "</div>\r\n";
},"useData":true});