"use strict";

var gulp = require("gulp");
var sass = require("gulp-sass");
var csso = require('gulp-csso');
var babel = require('gulp-babel');
var concat = require('gulp-concat');
var merge = require('merge-stream');

var babelConfig = {
    presets: ['@babel/env', '@babel/preset-react'],
    plugins: ['@babel/plugin-proposal-class-properties']
};

sass.compiler = require('node-sass');

gulp.task('fonts', function() {
    return gulp.src('node_modules/@fortawesome/fontawesome-free/webfonts/*')
        .pipe(gulp.dest('wwwroot/webfonts'));
});

gulp.task('sass', function () {
    return gulp.src("client/scss/bundle.scss")
        .pipe(sass().on('error', sass.logError))
        .pipe(gulp.dest('wwwroot/css'));
});

gulp.task('sass:watch', function () {
    gulp.watch('./client/scss/**/*.scss', ['sass', 'fonts']);
});

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

gulp.task('js:watch', function () {
    gulp.watch(['./client/js/**/*.js'], ['js']);
});

gulp.task('compile', ['fonts', 'sass', 'js']);

gulp.task('default', ['sass', 'sass:watch', 'js', 'js:watch']);