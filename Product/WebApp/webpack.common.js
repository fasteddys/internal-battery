module.exports = {
	entry: {
		components: './client/react/components/expose-components.js',
	},
	output: {
		filename: '[name].js',
		globalObject: 'this',
		path: __dirname + '/wwwroot/js',
	},
	optimization: {
		runtimeChunk: {
			name: 'runtime', // necessary when using multiple entrypoints on the same page
		},
		splitChunks: {
			cacheGroups: {
				commons: {
					test: /[\\/]node_modules[\\/]/,
					name: 'vendor',
					chunks: 'all',
				},
			},
		},
	},
	module: {
		rules: [
			{
				test: /\.jsx?$/,
				exclude: /node_modules/,
				loader: 'babel-loader'
			},
		],
	}
};