const path = require('path');
const fs = require('fs');

export default function(options) { 
    return {
        name: 'copy-worker',
        load() {
          this.addWatchFile(path.resolve('./dist/*'));
          this.addWatchFile(path.resolve('./copy.js'));
        },
        writeBundle() {
            console.log("copying built files...");
            let items = fs.readdirSync(path.resolve('./dist'));
            items.forEach(item => {
                let fullPath = path.resolve('./dist') + '/' + item;
                let targetPath = path.resolve('../BudgetTracker/wwwroot/')
                if (item.indexOf('.css') != -1)
                {
                    let targetName = targetPath + '/css/' + item;
                    fs.copyFileSync(fullPath, targetName);
                    console.log("Copying " + fullPath + " to " + targetName);
                }
                if (item.indexOf('.js') != -1)
                {
                    let targetName = targetPath + '/js/' + item;
                    fs.copyFileSync(fullPath, targetName);
                    console.log("Copying " + fullPath + " to " + targetName);
                }
            });
            console.log("files copied");
        }
    };
}