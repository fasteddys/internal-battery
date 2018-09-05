/// <binding Clean='clean' />
"use strict";

var gulp = require("gulp"),
  fs = require("fs"),
  sass = require("gulp-sass");

gulp.task("scss-compile", function () {
  return gulp.src("wwwroot/scss/index.scss")
    .pipe(sass())
    .pipe(gulp.dest("wwwroot/css"));
});