const path = require('path');
const fs = require('fs');
const process = require('process');
const cp = require("child_process");
const request = require('request');
const unzip = require("unzipper");
const rimraf = require("rimraf");

// --- VERSIONS
const protoVersion = "3.11.2";
const protoWebVersion = "1.0.7";

// --- INSTALL PROTOC STUFF

const platformExtension = (process.platform === "win32" ? ".exe" : "");

const protoDir = path.join(__dirname, "protoc");
const protoc_bin = path.join(protoDir, "bin");
const protoc = path.join(protoc_bin, "protoc" + platformExtension);

function downloadProtoc(cb) {
    if (!fs.existsSync(protoc)) {
        console.log("Downloading protoc...");
        if (!fs.existsSync(protoDir)) {
            fs.mkdirSync(protoDir);
        }
        
        var protoCreleases = {
        "win32": `https://github.com/google/protobuf/releases/download/v${protoVersion}/protoc-${protoVersion}-win32.zip`,
        "linux": `https://github.com/google/protobuf/releases/download/v${protoVersion}/protoc-${protoVersion}-linux-x86_64.zip`,
        };
    
        request(protoCreleases[process.platform])
        .pipe(unzip.Parse())
        .on("entry", function(entry) {
            var isFile = "File" === entry.type;
            var isDir = "Directory" === entry.type;
            var fullpath = path.join(protoDir, entry.path);
            var directory = isDir ? fullpath : path.dirname(fullpath);
    
            if (!fs.existsSync(directory)) {
                fs.mkdirSync(directory);
            }
    
            if (isFile) {
                entry.pipe(fs.createWriteStream(fullpath))
                .on("finish", function() {
                    if (protoc === fullpath) {
                        fs.chmodSync(fullpath, 0o755);
                    };
                });
            }
        })
        .on("close", cb);
    } else {
        cb();
    }
}

function downloadProtoGrpc(cb) {
    const genGrpcFullPath = path.join(protoc_bin, "protoc-gen-grpc-web" + platformExtension);
    if (!fs.existsSync(genGrpcFullPath)) {
        console.log("Downloading protoc-gen-grpc-web plugin...");
        var protoCwebReleases = {
            "win32": `https://github.com/grpc/grpc-web/releases/download/${protoWebVersion}/protoc-gen-grpc-web-${protoWebVersion}-windows-x86_64.exe`,
            "linux": `https://github.com/grpc/grpc-web/releases/download/${protoWebVersion}/protoc-gen-grpc-web-${protoWebVersion}-linux-x86_64`
        }
    
        request(protoCwebReleases[process.platform])
        .pipe(fs.createWriteStream(genGrpcFullPath))
        .on("close", () => {
            fs.chmodSync(genGrpcFullPath, 0o755);
            cb();
        })
    } else {
        cb();
    }
}

function generateCode(callback) {
    console.log("Generating code...");
    const protofilesDir = path.join(__dirname, "../BudgetTracker.Protocol");
    const outputDir = path.join(__dirname, "src/generated");
    
    if (fs.existsSync(outputDir)) {
        rimraf.sync(outputDir);
    }
    
    fs.mkdirSync(outputDir);
    
    cp.execFile(protoc, 
        [
            "-I=" + protofilesDir,
            "--js_out=import_style=commonjs:" + outputDir, 
            "--grpc-web_out=import_style=commonjs+dts,mode=grpcwebtext:" + outputDir
        ].concat(fs.readdirSync(protofilesDir)), 
        { env: { "PATH": protoc_bin } }, 
        function(err, stdout, stderr) {
            console.log("Completed code-gen")
            console.log(stdout);
            console.log(stderr);
            console.log("Finishing .proto code-gen...")
            callback();
        }
    );
}

export default function(options) { 
    return {
        name: 'generateProto',
        load() { 
            this.addWatchFile(path.resolve('../BudgetTracker.Protocol'));
            this.addWatchFile(path.resolve('./generate.js'));
        },
        async buildStart() { 
            console.log("Starting .proto code-gen...")
            return new Promise(resolve => {
                downloadProtoc(() => downloadProtoGrpc(() => generateCode(resolve)));
            })
        }
    };
}