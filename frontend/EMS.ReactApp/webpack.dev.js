import path from 'path';
import { merge } from 'webpack-merge';
import {config as commonConfig, __dirname} from './webpack.common.js';

export default merge(commonConfig, {
    mode: 'development',
    devServer: {
        static: {
            directory: path.join(__dirname, 'dist'),
        },
        compress: true,
        port: 5010,
        https: false,
        hot: false,
        setupMiddlewares: function (middlewares, devServer) {
            devServer.app.use('/', function (req, res,next) {
                console.log(`${(new Date()).toLocaleTimeString()} - from ${req.ip} - ${req.method} - ${req.originalUrl}`);
                next();
            });
            return middlewares;
        }
    },
    devtool: 'inline-source-map',
});