mergeInto(LibraryManager.library,
    { 
        InitBlockJs: function (key, id)
        {
            keyString = Pointer_stringify(key);
            idString = Pointer_stringify(id);

            if (!window.adBlocks)
            {
                window.adBlocks = new Map();
            }

            block = window.Adsgram.init(
            {
                blockId: idString,
                debug: true,
                debugBannerType: "FullscreenMedia"
            });

            window.adBlocks.set(keyString, block);
        },

        ShowBlockJs: function (key, id, goName, callbackName)
        {
            keyString = Pointer_stringify(key);
            idString = Pointer_stringify(id);
            goNameString = Pointer_stringify(goName);
            callbackNameString = Pointer_stringify(callbackName);

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