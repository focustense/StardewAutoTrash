<lane layout="50%[640..1200] 60%[500..]"
      orientation="vertical"
      horizontal-content-alignment="middle">
    <banner background={@Mods/StardewUI/Sprites/BannerBackground}
            background-border-thickness="48,0"
            padding="12"
            text={:Title} />
    <frame layout="stretch"
           background={@Mods/StardewUI/Sprites/MenuBackground}
           border={@Mods/StardewUI/Sprites/MenuBorder}
           border-thickness="36, 36, 40, 36">
        <scrollable *switch={:IsEmpty} layout="stretch" peeking="128">
            <label *case="true" padding="8" text={:EmptyText} />
            <grid *case="false"
                  layout="stretch content"
                  padding="16, 8"
                  item-layout="length: 64"
                  item-spacing="16, 16">
                <panel *repeat={:Items}
                       horizontal-content-alignment="end"
                       vertical-content-alignment="end"
                       tooltip={Tooltip}
                       left-click=|ToggleLocal()|
                       right-click=|ToggleGlobal()|>
                    <image layout="64px"
                           horizontal-alignment="middle"
                           vertical-alignment="middle"
                           sprite={:Sprite}
                           tint={Tint}
                           shadow-alpha={ShadowAlpha}
                           shadow-offset="-4, 4"
                           focusable="true" />
                    <panel *switch={IsLocalTrash} layout="18px">
                        <image *case="false"
                               layout="stretch"
                               sprite={@Mods/focustense.GarbageInGarbageCan/Sprites/UI:TrashOff}
                               tint="#3333" />
                        <image *case="true"
                               layout="stretch"
                               sprite={@Mods/focustense.GarbageInGarbageCan/Sprites/UI:TrashOn} />
                    </panel>
                    <image *if={IsGlobalTrash}
                           layout="18px"
                           margin="0, 0, 24, 0"
                           sprite={@Mods/focustense.GarbageInGarbageCan/Sprites/UI:TrashGlobal} />
                </panel>
            </grid>
        </scrollable>
    </frame>
    <frame margin="0, 8, 0, 0" padding="24" background={@Mods/StardewUI/Sprites/ControlBorder}>
        <lane vertical-content-alignment="middle">
            <label bold="true" text={#TrashMenu.Legend.Title} />
            <image layout="27px"
                   margin="24, 0, 8, 0"
                   sprite={@Mods/focustense.GarbageInGarbageCan/Sprites/UI:TrashOn} />
            <label text={#TrashMenu.Legend.Local} />
            <image layout="27px"
                   margin="16, 0, 8, 0"
                   sprite={@Mods/focustense.GarbageInGarbageCan/Sprites/UI:TrashGlobal} />
            <label text={#TrashMenu.Legend.Global} />
        </lane>
    </frame>
</lane>