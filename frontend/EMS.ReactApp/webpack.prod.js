import { merge } from 'webpack-merge';
import {config as commonConfig, __dirname} from './webpack.common.js';

export default merge(commonConfig, {
    mode: 'production',
    devtool: 'source-map',
});
