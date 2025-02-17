mergeInto(LibraryManager.library,
    { 
        InitBlockJs: function (key, id, debugMode)
        {
            keyString = UTF8ToString(key);
            idString = UTF8ToString(id);

            if (!window.adBlocks)
            {
                window.adBlocks = new Map();
            }

            block = window.Adsgram.init(
            {
                blockId: idString,
                debug: (debugMode == 1),
                debugBannerType: "FullscreenMedia"
            });

            window.adBlocks.set(keyString, block);
        },

        ShowBlockJs: function (key, id, goName, callbackName)
        {
            keyString = UTF8ToString(key);
            idString = UTF8ToString(id);
            goNameString = UTF8ToString(goName);
            callbackNameString = UTF8ToString(callbackName);

            block = window.adBlocks.get(keyString);
            if (block)
            {
                block.show()
                .then((result) => 
                {
                    window.unityInstance.SendMessage(goNameString, callbackNameString, `${id}/complete`);
                })
                .catch((result) => 
                {
                    window.unityInstance.SendMessage(goNameString, callbackNameString, `${id}/canceled`);
                });     
            }
        },
    }
);