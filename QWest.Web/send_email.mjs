import sendmailInit from "sendmail"

//TODO: for later optimization, run this as a seperate service so the we dont have to initalize all the time

const sendMail = sendmailInit({
    silent: true
});
const commandLineArg = process.argv.slice(3)
const options = JSON.parse(commandLineArg[0]);
sendMail(options, (err, reply) => {
    if (err) {
        throw err
    }
})