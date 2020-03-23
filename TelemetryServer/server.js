//jshint esverion:6

const express = require("express");

const app = express();

const port = 3000;

app.get("/", function(req, res){
  res.send("<h1>Hello World</h1>");
});

app.get("/test", function(req, res){
  res.send("<h1>test route</h1>");
});


app.listen(port, function(){
  console.log("Server started on port " + port);
});
