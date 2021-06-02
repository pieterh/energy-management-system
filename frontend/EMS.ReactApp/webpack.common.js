import path from 'path';
import { fileURLToPath } from 'url';
import HtmlWebpackPlugin from 'html-webpack-plugin';
import InterpolateHtmlPlugin from 'interpolate-html-plugin';
import CopyWebpackPlugin from 'copy-webpack-plugin';

const publicUrl = '/app';
export const __dirname = path.dirname(fileURLToPath(import.meta.url));

export const config = {
    plugins : [
        new CopyWebpackPlugin({
            patterns: [
                { from: 'public/logo192.png', to: '' },
                { from: 'public/logo512.png', to: '' },
                { from: 'public/app.webmanifest', to: '' },
            ]
        }),
        new InterpolateHtmlPlugin({
            PUBLIC_URL: publicUrl,
        }),        
        new HtmlWebpackPlugin({
            title: 'ReactApp',
            favicon: 'public/favicon.ico',
            template: 'public/index.html',
        }),
    ],
    entry : [
        './src/index.tsx',
    ],
    target : 'browserslist',
    output : {
        filename: '[name].bundle.[chunkhash].js',
        path: path.resolve(__dirname, 'dist/app'),
        publicPath: "/app/",
        clean: true
    },
    resolve : {
        extensions: ['.tsx', '.ts', '.js', '.mjs'],
    },
    module : {
        rules: [
            {
                test: /\.(js)$/,
                exclude: /node_modules/,
                use: {
                    loader: 'babel-loader',
                    options: {
                        presets: ['@babel/preset-env']
                    }
                }
            },
            {
                test: /\.m?js$/,
                exclude: /node_modules/,
                resolve: {
                    fullySpecified: false
                }
            },
            {
                test: /\.tsx?$/,
                use: {
                    loader: 'ts-loader',
                },
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
                test: /\.(ttf|eot|woff(2)?)(\?[a-z0-9=&.]+)?$/,
                loader: 'file-loader'
            }
        ],
    }
};
