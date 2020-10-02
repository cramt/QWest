import connect from "connect";
import serveStatic from "serve-static"
import webpack from "webpack"
import HtmlWebpackPlugin from "html-webpack-plugin"
import fs from "fs"
import CopyWebpackPlugin from "copy-webpack-plugin"
import HttpProxyMiddleware from "http-proxy-middleware"
import { killProcessOnPort } from "./utilities.mjs";

const port = 8080;

const subProjects = fs.readdirSync("frontend")
    .filter(x => fs.lstatSync("frontend/" + x).isDirectory())
    .filter(x => fs.existsSync("frontend/" + x + "/index.html") && fs.existsSync("frontend/" + x + "/index.js"))

const webpackConfig = {
    mode: 'development',
    entry: subProjects.reduce((acc, x) => {
        acc[x] = "./frontend/" + x + "/index.js"
        return acc
    }, {}),
    output: {
        filename: '[name].[contentHash].bundle.js'
    },
    plugins: subProjects.map(x => new HtmlWebpackPlugin({
        filename: x + ".html",
        template: "./frontend/" + x + "/index.html",
        chunks: [x]
    })).concat([
        new CopyWebpackPlugin({
            patterns: [
                {
                    from: "public",
                }
            ]
        })
    ]),
};

(async () => {
    const compiler = webpack(webpackConfig);
    const watcher = compiler.watch({}, err => {
        if (err) {
            console.log("error")
            console.log(err)
        }
        else {
            console.log("webpack finished compiling")
        }
    });
})().catch(console.log);

(async () => {
    await killProcessOnPort(port)

    connect()
        .use("/api", HttpProxyMiddleware.createProxyMiddleware({ target: "http://localhost:9000/", changeOrigin: true }))
        .use(serveStatic("./dist/"))
        .listen(port)
})().catch(console.log);