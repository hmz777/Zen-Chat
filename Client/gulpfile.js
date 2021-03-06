'use strict';

var gulp = require('gulp'),
    concat = require('gulp-concat'),
    cssmin = require('gulp-cssmin'),
    sass = require('gulp-sass'),
    cleancss = require('gulp-clean-css'),
    postcss = require('gulp-postcss'),
    autoprefixer = require('autoprefixer'),
    htmlmin = require('gulp-htmlmin'),
    terser = require('gulp-terser'),
    merge = require('merge-stream'),
    del = require('del'),
    wait = require('gulp-wait'),
    bundleconfig = require('./bundleconfig.json');

sass.compiler = require('node-sass');

const regex = {
    css: /\.css$/,
    html: /\.(html|htm)$/,
    js: /\.js$/
};

gulp.task('min:js', async function () {
    merge(getBundles(regex.js).map(bundle => {
        return gulp.src(bundle.inputFiles, { base: '.' })
            .pipe(concat(bundle.outputFileName))
            .pipe(terser())
            .pipe(gulp.dest('.'));
    }))
});

gulp.task('min:css', async function () {
    merge(getBundles(regex.css).map(bundle => {
        return gulp.src(bundle.inputFiles, { base: '.' })
            .pipe(wait(200))             
            .pipe(concat(bundle.outputFileName))//.pipe(sass({ outputStyle: "compressed" }))             
            .pipe(postcss([autoprefixer()]))
            .pipe(cleancss({ compatibility: 'ie8' }))
            .pipe(gulp.dest('.'));
    }))
});

gulp.task('min:html', async function () {
    merge(getBundles(regex.html).map(bundle => {
        return gulp.src(bundle.inputFiles, { base: '.' })
            .pipe(concat(bundle.outputFileName))
            .pipe(htmlmin({ collapseWhitespace: true, minifyCSS: true, minifyJS: true }))
            .pipe(gulp.dest('.'));
    }))
});

gulp.task('min', gulp.series(['min:js', 'min:css', 'min:html']));

gulp.task('clean', () => {
    return del(bundleconfig.map(bundle => bundle.outputFileName));
});

gulp.task('watch', () => {
    // getBundles(regex.js).forEach(
    //     bundle => gulp.watch(bundle.inputFiles, gulp.series(["min:js"])));

    getBundles(regex.css).forEach(
        bundle => gulp.watch("Assets/sass/*.scss", gulp.series(["min:css"])));

    // getBundles(regex.html).forEach(
    //     bundle => gulp.watch(bundle.inputFiles, gulp.series(['min:html'])));
});

const getBundles = (regexPattern) => {
    return bundleconfig.filter(bundle => {
        return regexPattern.test(bundle.outputFileName);
    });
};

gulp.task('default', gulp.series("watch"));