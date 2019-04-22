const merge = require('webpack-merge');
const common = require('./webpack.common.js');
const UglifyJsPlugin = require('uglifyjs-webpack-plugin');

module.exports = merge(common, {
    optimization: {
        minimizer: [
            new UglifyJsPlugin()
        ]
    },
    mode: 'production',
    devtool: 'nosources-source-map'
});