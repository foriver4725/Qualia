/////////////////////////////////////////////////////////////////////////////////////////////////////
//
// Audiokinetic Wwise generated include file. Do not edit.
//
/////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef __WWISE_IDS_H__
#define __WWISE_IDS_H__

#include <AK/SoundEngine/Common/AkTypes.h>

namespace AK
{
    namespace EVENTS
    {
        static const AkUniqueID PLAY_BUTTON = 2099555730U;
        static const AkUniqueID PLAY_DISASTER = 1755577099U;
        static const AkUniqueID PLAY_SOS = 2990131299U;
        static const AkUniqueID PLAY_TRIGGER = 166031882U;
        static const AkUniqueID PLAY_WALK = 1589278981U;
        static const AkUniqueID STOP_DISASTER = 441389605U;
        static const AkUniqueID STOP_WALK = 3140964691U;
    } // namespace EVENTS

    namespace SWITCHES
    {
        namespace BUTTON
        {
            static const AkUniqueID GROUP = 977454165U;

            namespace SWITCH
            {
                static const AkUniqueID CLICK = 1584507803U;
                static const AkUniqueID HOVER = 3753593413U;
            } // namespace SWITCH
        } // namespace BUTTON

        namespace CHARACTERTRIGGER
        {
            static const AkUniqueID GROUP = 1625523386U;

            namespace SWITCH
            {
                static const AkUniqueID BEGIN = 349818688U;
                static const AkUniqueID END = 529726532U;
            } // namespace SWITCH
        } // namespace CHARACTERTRIGGER

        namespace DISASTER
        {
            static const AkUniqueID GROUP = 1888685156U;

            namespace SWITCH
            {
                static const AkUniqueID BLIZZARD = 3610151219U;
                static const AkUniqueID WINDSTORM = 3009375806U;
            } // namespace SWITCH
        } // namespace DISASTER

        namespace SOS
        {
            static const AkUniqueID GROUP = 544238338U;

            namespace SWITCH
            {
                static const AkUniqueID NOTREMOVE = 1444079976U;
                static const AkUniqueID REMOVE = 2335605169U;
            } // namespace SWITCH
        } // namespace SOS

        namespace WALK
        {
            static const AkUniqueID GROUP = 2108779966U;

            namespace SWITCH
            {
                static const AkUniqueID GRASS = 4248645337U;
                static const AkUniqueID ROCK = 2144363834U;
                static const AkUniqueID SAND = 803837735U;
                static const AkUniqueID WATER = 2654748154U;
            } // namespace SWITCH
        } // namespace WALK

    } // namespace SWITCHES

    namespace GAME_PARAMETERS
    {
        static const AkUniqueID WALKPITCH = 1559442200U;
    } // namespace GAME_PARAMETERS

    namespace BANKS
    {
        static const AkUniqueID INIT = 1355168291U;
        static const AkUniqueID MAIN = 3161908922U;
    } // namespace BANKS

    namespace BUSSES
    {
        static const AkUniqueID BGM = 412724365U;
        static const AkUniqueID MAIN_AUDIO_BUS = 2246998526U;
        static const AkUniqueID SE = 1584861537U;
        static const AkUniqueID WALK = 2108779966U;
    } // namespace BUSSES

    namespace AUDIO_DEVICES
    {
        static const AkUniqueID NO_OUTPUT = 2317455096U;
        static const AkUniqueID SYSTEM = 3859886410U;
    } // namespace AUDIO_DEVICES

}// namespace AK

#endif // __WWISE_IDS_H__
