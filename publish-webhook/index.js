const webhook = require("webhook-discord");
const util = require('util');
const exec = util.promisify(require('child_process').exec);
const fs = require('fs').promises;

let msg = new webhook.MessageBuilder();

for (let j = 2; j < process.argv.length - 1; j += 2) {
    console.log(j + ' -> ' + (process.argv[j]));
    msg = msg.addField(process.argv[j], process.argv[j + 1]);
}

msg = msg
    .setName("Publisher Bot")
    .setColor("#aa00cc")
    .setText("Published")
    // .setImage("https://is2-ssl.mzstatic.com/image/thumb/Purple113/v4/93/2d/1c/932d1cc2-45c1-ca8b-4d56-9aa4a1cc3950/source/256x256bb.jpg")
    .setTime();

function truncate(s) {
    if (s.length > 1000) {
        return s.substring(0, 986) + '...(truncated)';
    } else {
        return s;
    }
}

async function main() {
    const { stdout: currentDeploymentPath, stderr: stderr1 } = await exec('cmd /c "cscript.exe /Nologo %systemdrive%\\inetpub\\adminscripts\\adsutil.vbs get /w3svc/1/ROOT/Path | cut -c45- | rev | cut -c3- | rev"', { shell: true });
    console.log('Current website served from', currentDeploymentPath);
    msg = msg.addField('Current website served from', currentDeploymentPath);
    const { stdout: currentCommit, stderr: stderr2 } = await exec('git log --oneline | head -1', { shell: true });
    console.log('Current commit', currentCommit);
    msg = msg.addField('Current commit', truncate(currentCommit));
    const { stdout: gitStatus, stderr: stderr3 } = await exec('git status --short', { shell: true });
    console.log('Git Status', gitStatus);
    msg = msg.addField('Git Status', truncate(gitStatus));
    const { stdout: gitDiff, stderr: stderr4 } = await exec('git diff HEAD', { shell: true });
    console.log('Git Diff', gitDiff);
    msg = msg.addField('Git Diff', truncate(gitDiff));
    const hookUrl = await fs.readFile('hookurl.txt', 'utf8');
    console.log('hook url', hookUrl);
    const Hook = new webhook.Webhook(hookUrl);
    Hook.send(msg);
}

main();
