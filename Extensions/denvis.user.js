// ==UserScript==
// @name         DenVis Extension
// @namespace    https://denvis.glitch.me/
// @version      0.1
// @description  Integrates DenVis to Youtube and other media
// @author       TheAlan404 (Dennis)
// @include      http://*
// @include      https://*
// ==/UserScript==

const config = {
    // Show text only when you arent seeing the page
    onlyWhenHidden: true
}

const denvis = (() => {
    let socket;
    let listeners = {};
    let info = null;

    const setup = () => {
        socket = new WebSocket("ws://localhost:4242");
        socket.addEventListener("close", () => setTimeout(setup, 3000));
        socket.addEventListener("message", (m) => {
            let packet = JSON.parse(m);
            emit(packet.name, packet.data);
            emit("packet", packet);
        });
    };

    const send = (name = "", data = null) => {
        do {
            //
        } while(socket.readyState !== 1);
        socket.send(JSON.stringify({ name, data }));
    };
    const on = (name, listener) => {
        if(!listeners[name]) listeners[name] = [];
        listeners[name].push(listener);
    };
    const emit = (name, data) => {
        if(!listeners[name]) return;
        listeners[name].forEach(l => l(data));
    };

    setup();

    on("welcome", (i) => info = i);

    return {
        send,
        socket,
        on,
        get info() {
            return info;
        }
    };
})();

(function() {
    'use strict';

    const musicEmoji = "🎵";

    const injectYoutube = () => {
        document.addEventListener("yt-navigate-finish", (e) => {
            let info = e.detail.response.playerResponse.videoDetails;

            info.author = info.author.replace("- Topic", ""); // todo, replace with splice idk

            if(config.onlyWhenHidden && !document.hidden) return;

            denvis.send("AddText", {
                Text: musicEmoji + " Now Playing: " + info.title + "\nby: " + info.author,
                Expire: 7 * 1000,
                X: 20,
                Y: 20
            });
        });
    }

    const run = () => {
        if(window.location.host === "www.youtube.com") {
            injectYoutube();
        };
        
        console.log("[DenVisExt] Running");
    }

    if(document.readyState === "complete") run();
    else window.addEventListener("load", run);
})();