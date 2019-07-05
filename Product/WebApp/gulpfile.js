"use strict";

// import dependencies
var gulp = require("gulp");
var sass = require("gulp-sass");
var csso = require('gulp-csso');
var babel = require('gulp-babel');
var concat = require('gulp-concat');
var merge = require('merge-stream');

// babel config for javascript transpilation to browser capable javascript
var babelConfig = {
    presets: ['@babel/env', '@babel/preset-react'],
    plugins: ['@babel/plugin-proposal-class-properties']
};

// set sass compiler
sass.compiler = require('node-sass');

// copy fonts from node_modules into web servable directory
gulp.task('fonts', function() {
    return gulp.src('node_modules/@fortawesome/fontawesome-free/webfonts/*')
        .pipe(gulp.dest('wwwroot/webfonts'));
});

// compile sass task
gulp.task('sass', function () {
    return gulp.src("client/scss/bundle.scss")
        .pipe(sass().on('error', sass.logError))
        .pipe(gulp.dest('wwwroot/css'));
});

// sass task with watcher (compiles on save)
gulp.task('sass:watch', function () {
    gulp.watch('./client/scss/**/*.scss', ['sass', 'fonts']);
});

// transpile and concatenate js
gulp.task('js', function () {
    var core = gulp.src([
            './client/js/core/**/*.js'
        ])
        .pipe(babel(babelConfig))
        .pipe(concat('core.js'))
        .pipe(gulp.dest('./client/dist/js'));

    var modules = gulp.src([
            './client/js/modules/**/*.js'
        ])
        .pipe(babel(babelConfig))
        .pipe(gulp.dest('./wwwroot/js/modules'));

    return merge(core, modules);
});

// watcher for javascript task
gulp.task('js:watch', function () {
    gulp.watch(['./client/js/**/*.js'], ['js']);
});

// aggregate task for compiling/executing required tasks but only once
gulp.task('compile', ['fonts', 'sass', 'js']);

// aggregate task for compiling/executing required tasks but with watcher
gulp.task('default', ['sass', 'sass:watch', 'js', 'js:watch']);