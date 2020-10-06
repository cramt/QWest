import connect from "connect";
import serveStatic from "serve-static"
import webpack from "webpack"
import HtmlWebpackPlugin from "html-webpack-plugin"
import fs from "fs"
import CopyWebpackPlugin from "copy-webpack-plugin"
import HttpProxyMiddleware from "http-proxy-middleware"

const commandLineArg = process.argv.slice(3)

const port = commandLineArg[0];
const apiPort = commandLineArg[1];

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

connect()
    .use("/api", HttpProxyMiddleware.createProxyMiddleware({ target: `http://localhost:${apiPort}/`, changeOrigin: true }))
    .use(serveStatic("./dist/"))
    .listen(8080)