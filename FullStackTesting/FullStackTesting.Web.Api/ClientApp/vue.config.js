/* eslint-disable no-param-reassign,import/no-extraneous-dependencies  */

module.exports = {
  devServer: {
    port: 5001,
    proxy: {
      '/api': {
          target: 'http://localhost:5000',
          changeOrigin: true
      },
      '/swagger': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      }
    }
  },
  configureWebpack: (config) => {
    if (process.env.NODE_ENV !== 'production') {
      return {};
    }

    return {
      devtool: false,
      performance: {
        hints: false,
        maxEntrypointSize: 512000,
        maxAssetSize: 512000
      },
      plugins: [],
    };
  },
};