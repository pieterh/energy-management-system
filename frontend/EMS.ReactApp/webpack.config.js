path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const InterpolateHtmlPlugin = require('interpolate-html-plugin');
const CopyWebpackPlugin = require('copy-webpack-plugin');

const publicUrl = '/app';

module.exports = {
    entry: {
        index: './src/index.tsx',
    },
    plugins: [       
        new CopyWebpackPlugin({
            patterns: [
                { from: 'public/logo192.png', to: '' },
                { from: 'public/logo512.png', to: '' },
                { from: 'public/manifest.json', to: '' },
            ]
        }),
        new InterpolateHtmlPlugin({
            PUBLIC_URL: publicUrl,
        }),        
        new HtmlWebpackPlugin({
            title: 'ReactApp',
            favicon: 'public/favicon.ico',
            template: 'public/index.html',
        })
    ],
    output: {
        filename: '[name].bundle.[chunkhash].js',
        path: path.resolve(__dirname, 'dist/app'),
        publicPath: "/app/",
        clean: true
    },
    resolve: {
        extensions: ['.tsx', '.ts', '.js'],
    },
    module: {
        rules: [
            {
                test: /\.(js)$/,
                exclude: /node_modules/,
                use: ['babel-loader']
            },
            {
                test: /\.tsx?$/,
                use: 'ts-loader',
                exclude: /node_modules/,
            },            
            {
                test: /\.css$/i,
                use: [
                    {
                        loader: 'style-loader'
                    },
                    {
                        loader: 'css-loader',
                        options: {
                            modules: true,
                            sourceMap: true
                        }
                    }
                ],
            },
            {
                test: /\.svg$/,
                use: [
                    {
                      loader: 'svg-url-loader',
                      options: {
                        limit: 10000,
                      },
                    },
                  ],
            },
            {
                test   : /\.(ttf|eot|woff(2)?)(\?[a-z0-9=&.]+)?$/,
                loader : 'file-loader'
            }
        ],
    },
    mode: 'development',
    devServer: {
        contentBase: path.join(__dirname, 'dist'),
        compress: true,
        port: 5010,
        https: false,       
        before: function (app, server, compiler) {
            app.use('/', function (req, res,next) {
                console.log(`${(new Date()).toLocaleTimeString()} - from ${req.ip} - ${req.method} - ${req.originalUrl}`);
                next();
            });
        }
    },
};
