"use strict";

var gulp = require("gulp");
var sass = require("gulp-sass");
var csso = require('gulp-csso');

sass.compiler = require('node-sass');

gulp.task('sass', function () {
    return gulp.src("wwwroot/scss/bundle.scss")
        .pipe(sass().on('error', sass.logError))
        .pipe(gulp.dest('wwwroot/css'));
});

gulp.task('sass:watch', function () {
    gulp.watch('./wwwroot/scss/**/*.scss', ['sass']);
});