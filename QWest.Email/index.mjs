import express from "express"
import sendmailInit from "sendmail"
import fs from "fs"
import path from "path"
import { fileURLToPath } from 'url';
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const config = JSON.parse(fs.readFileSync(path.resolve(__dirname, "../Config/config.json")).toString())

const port = config.email_port;

const sendMailRaw = sendmailInit({

});

const sendMail = (arg) => {
    return new Promise((resolve, reject) => {
        sendMailRaw(arg, (err, reply) => {
            if (err) {
                reject(err)
            }
            else {
                resolve(reply)
            }
        })
    })
}

const app = express();

app.use(express.json());

app.post("/send", async (req, res) => {
    const response = await sendMail(req.body);
    res.status(200).send(response)
})

app.listen(port, () => {
    console.log(`email service listening at http://localhost:${port}`)
})