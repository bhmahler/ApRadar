Imports System.Runtime.InteropServices
Imports System.Drawing
Imports System.Xml.Serialization
Public Class MobData
    Implements IDisposable

#Region " MEMBER VARIABLES "
    Private _isPacket As Boolean
    'Private _mobPacket As NPCPacket
    Private _mapID As Byte
    Private _zones As Zones
    Private _dataPreLoaded As Boolean = False
#End Region

#Region " CONSTRUCTORS "

    Protected Sub New()
    End Sub

    Public Sub New(ByVal POL As Process, ByVal MobBase As Integer, ByVal PreloadData As Boolean)
        _mobBase = MobBase
        _pol = POL
        _isPacket = False
        If PreloadData Then
            _dataPreLoaded = True
            MobBlock = MemoryObject.GetByteArray(450)
        End If
    End Sub
#End Region

#Region " ENUMERATIONS "
    Public Enum MobTypes
        PC = 0
        NPC = 1
        NPCUnique = 2 'May just mean can interact. not sure yet
        NPCObject = 3 'so far only doors...
        Boat = 5
    End Enum

    ''' <summary>
    ''' This is the type of model it is.  This is a bitflag so it can me a combination of many.
    ''' </summary>
    ''' <remarks>This may come in quite handy</remarks>
    <Flags()>
    Public Enum SpawnTypes
        None = 0
        PC = 1
        NPC = 2
        GroupMember = 4
        AllianceMember = 8
        Mob = 16
        DoorOrObject = 32 'May just be an interactable object
        Unk1 = 64
        Unk2 = 128
    End Enum

    Public Enum MobOffsets
        LastX = 4
        LastY = 12
        LastZ = 8
        LastDirection = 24
        X = 36
        Y = 44
        Z = 40
        Direction = 56
        ID = 116
        ServerID = 120
        Name = 124
        WarpInfo = 160 '12/12/12 8 bytes added after name
        '4 byte integer added 3/26/2013
        Distance = 176
        ' -8 bytes here
        TP = 196
        HP = 200
        MobType = 203 '198
        Race = 204 '199
        AttackTimer = 205
        Face = 208
        Hair = 220 '7/12/11 Start of 4 byte shift (not sure what they added)
        Head = 222
        Body = 224
        Hands = 226
        Legs = 228
        Feet = 230
        MainWeapon = 232
        SubWeapon = 234
        '12/12/12 Shifted -4 after render somewhere between these 2
        PIcon = 264
        GIcon = 268
        Speed = 308
        Status = 324
        Status2 = 328
        ClaimedBy = 348 'Was 340
        SpawnType = 408 'Was 400
        PCTarget = 448
        PetIndex = 450
    End Enum


    '    //Biggest. Struct. Ever.
    '[StructLayout(LayoutKind.Sequential, Pack = 1)] //im sure some of these fields could be removed with the proper packing. but im lazy.
    'public struct SpawnInfo {
    '   public Int32 u1; //possibly a signature. always seems to be this for player records. other bytes noticed here for different data types in the linked list.
    '   public float PredictedX; //These coords jump. a LOT. This leaves me to believe they are predicted values by the client.
    '   public float PredictedZ; //  Prediction is rooted in FPS games to give the user a smoother movement experience.
    '   public float PredictedY; //  Lag in whitegate will GREATLY demonstrate the properties of these values if used.
    '   public float u2;
    '   public Int32 u3;
    '   public float PredictedHeading;
    '   public Int32 u4;
    '   public float u5;
    '   public float X; //These coords are used because it seems like a good mix between actual and predicted.
    '   public float Z; //Also note that the assinine ordering (xzy) is corrected.
    '   public float Y;
    '   public float u6;
    '   public Int32 u7;
    '   public float Heading; //heading is expressed in radians. cos/sin will extract a normalized x/y coord.
    '   public Int32 u8;
    '   public Int32 u9;
    '   public float X2; //These are assumed to be server confirmed (actual) coords.
    '   public float Z2;
    '   public float Y2;
    '   public float u10;
    '   public Int32 u11;
    '   public Int32 u12;
    '   public Int32 u13;
    '   public Int32 u14;
    '   public Int32 u15;
    '   public Int32 u16;
    '   public Int32 u17;
    '   public Int32 u18;
    '   public UInt32 ZoneID;
    '   public UInt32 ServerID;
    '   [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 24)]
    '   public string DisplayName; //player is 16 max, but npc's can be up to 24.
    '   public Int32 pUnknown;
    '   public float RunSpeed;
    '   public float RunSpeed2;
    '   public Int32 pListNode; //a pointer to the node this record belongs to in a resource linked list. note that the data in this list contains many things not just spawn data. further down the chain is unstable.
    '   public Int32 u19;
    '   public Int32 NPCTalking;
    '   public float Distance;
    '   public Int32 u20;
    '   public Int32 u21;
    '   public float Heading2;
    '   public Int32 pPetOwner; //only for permanent pets. charmed mobs do not fill this.
    '   public Int32 u22;
    '   public byte HealthPercent;
    '   public byte u23;
    '   public byte ModelType;
    '   public byte Race;
    '   public Int32 NPCPathingTime;
    '   public Int32 u25;
    '   public Int32 u26;
    '   public Int16 Model;
    '   public Int16 ModelHead;
    '   public Int16 ModelBody;
    '   public Int16 ModelHands;
    '   public Int16 ModelLegs;
    '   public Int16 ModelFeet;
    '   public Int16 ModelMain;
    '   public Int16 ModelSub;
    '   public Int16 ModelRanged;
    '   public Int32 u27;
    '   public Int32 u28;
    '   public Int32 u29;
    '   public Int16 u30;
    '   public Int16 u31;
    '   public byte u33;
    '   public byte u34;
    '   public Int32 u70; //Added Feb 14th 2011 Patch; Fixed by Jetsam
    '   public byte FlagRender;
    '   public byte Flags1; //I am well aware these should be combined,
    '   public byte Flags2; //  but it is easier for future discoveries
    '   public byte Flags3; //  as my documentation and memory dissect
    '   public byte Flags4; //  structs have these separated (to spot
    '   public byte Flags5; //  flag changes after performing an action).
    '   public byte Flags6;
    '   public byte Flags7; //  besides, I am not entirely sure where the
    '   public byte Flags8; //  first flag boundary starts, and what lies
    '   public byte Flags9; //  on a word boundary (and thus is padding).
    '   public Int32 u71; //Added Jul 13th 2011 Patch; Fixed by Zer0Blues
    '   public byte Flags10;
    '   public byte Flags11;
    '   public byte Flags12;
    '   public byte Flags13;
    '   public byte Flags14;
    '   public byte Flags15;
    '   public byte Flags16;
    '   public byte Flags17;
    '   public byte Flags18;
    '   public byte Flags19;
    '   public byte Flags20;
    '   public byte u35;
    '   public Int32 u36;
    '   public Int32 u37;
    '   public Int16 NPCSpeechLoop;
    '   public Int16 NPCSpeechFrame;
    '   public Int32 u38;
    '   public Int32 u39;
    '   public Int16 u40;
    '   public float RunSpeed3;
    '   public Int16 NPCWalkPos1;
    '   public Int16 NPCWalkPos2;
    '   public Int16 NPCWalkMode;
    '   public Int16 u41;
    '   [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
    '   public string mou4; //always this. assuming an animation name
    '   public UInt32 FlagsCombat;
    '   public UInt32 FlagsCombatSVR; //im assuming this is updated after the client asks the server. there is a noticable delay between the two
    '   public Int32 u42;
    '   public Int32 u43;
    '   public Int32 u44;
    '   public Int32 u45;
    '   public Int32 u46;
    '   public UInt32 ClaimID; //the SERVER id of the player that has claim on the mob. the claim will bounce around to whomever has the most hate. not exactly the same as /assist but it will have to do.
    '   public Int32 u47;
    '   [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
    '   public string Animation1;
    '   [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
    '   public string Animation2;
    '   [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
    '   public string Animation3;
    '   [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
    '   public string Animation4;
    '   [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
    '   public string Animation5;
    '   [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
    '   public string Animation6;
    '   [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
    '   public string Animation7;
    '   [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
    '   public string Animation8;
    '   [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
    '   public string Animation9;
    '   public Int16 AnimationTime; //guessed, but something to do with the current animation
    '   public Int16 AnimationStep; //guessed, but something to do with the current animation
    '   public Int16 u48;
    '   public Int16 u49;
    '   public UInt32 EmoteID;
    '   [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
    '   public string EmoteName;
    '   public byte SpawnType;
    '   public byte u50;
    '   public Int16 u51;
    '   public byte LSColorRed;
    '   public byte LSColorGreen;
    '   public byte LSColorBlue;
    '   public byte u52;
    '   public byte u53;
    '   public byte u54;
    '   public byte CampaignMode; //boolean value. 
    '   public byte u55;
    '   public byte u56;
    '   public byte u57;
    '   public Int16 u58;
    '   public Int16 u59;
    '   public Int16 u60;
    '   public Int16 u61;
    '   public Int16 u62;
    '   public Int16 u63;
    '   public Int16 u64;
    '   [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
    '   public string TalkAnimation;
    '   public Int32 u65;
    '   public Int32 u66;
    '   public UInt32 PetID; //the zone id of the spawn considered this spawns pet.
    '   public Int16 u67;
    '   public byte u68;
    '   public byte u69;
    '}
#End Region

#Region " STRUCTURES "
    ''' <summary>
    ''' Mob structure
    ''' </summary>
    <StructLayout(LayoutKind.Sequential)> _
    Public Structure MobInfo
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=4)> _
        Public Unknown1 As Byte() '0
        Public LastX As Single '4
        Public LastZ As Single '8
        Public LastY As Single '12
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=8)> _
        Public Unknown2 As Byte() '16
        Public LastDirection As Single '24
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=8)> _
        Public Unknown3 As Byte() '28
        Public PosX As Single '36
        Public PosZ As Single '40
        Public PosY As Single '44
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=8)> _
        Public Unknown4 As Byte() '48
        '11-19-2007 Moved to 56 from 60
        Public PosDirection As Single '56
        '11-19-2007 added 4 bytes always 0
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=56)> _
        Public Unknown5 As Byte() '60
        Public ID As Integer '116
        Public ServerCharId As Integer '120
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=24)> _
        Public MobName As String '124
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=12)> _
        Public Unknown6 As Byte() '144
        Public WarpInfo As Integer '160 WarpStruct Pointer
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=8)> _
        Public Unknown7 As Byte() '164
        Public distance As Single '172
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=16)> _
        Public Unknown8 As Byte() '176
        Public TP_Percent As Short '192
        Public Unknown9 As Short '194
        Public HP_Percent As Byte '196
        Public Unknown10 As Byte '197
        Public MobType As Byte '198
        Public Race As Byte '199
        Public Unknown11 As Byte '200
        Public AttackTimer As Byte '201
        Public Unknown12 As Short '202
        Public Fade As Byte '204
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=7)> _
        Public Unknown13 As Byte() '205
        Public Hair As Short '212
        Public Head As Short '214
        Public Body As Short '216
        Public Hands As Short '218
        Public Legs As Short '220
        Public Feet As Short '222
        Public Main As Short '224
        Public [Sub] As Short '226
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=24)> _
        Public Unknown14 As Byte() '228
        Public pIcon As Byte '252
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=3)> _
        Public Unknown15 As Byte() '253 
        Public gIcon As Short '256
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=34)> _
        Public Unknown16 As Byte() '258
        Public Speed As Single '292
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=12)> _
        Public Unknown17 As Byte() '296
        '294 -- 296 Mob Moving short 298 same -- 300 8 when not moving
        Public Status As Integer '308
        Public Status2 As Integer '312
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=20)> _
        Public Unknown18 As Byte() '316
        Public ClaimedBy As Integer '336
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=90)> _
        Public Unknown19 As Byte() '340
        Public PetIndex As Short '426
    End Structure

    'Biggest. Struct. Ever.
    'im sure some of these fields could be removed with the proper packing. but im lazy.
    <StructLayout(LayoutKind.Sequential, Pack:=1)> _
    Public Structure SpawnInfo
        Public u1 As Int32 '0
        'possibly a signature. always seems to be this for player records. other bytes noticed here for different data types in the linked list.
        Public PredictedX As Single '4
        'These coords jump. a LOT. This leaves me to believe they are predicted values by the client.
        Public PredictedZ As Single '8
        '  Prediction is rooted in FPS games to give the user a smoother movement experience.
        Public PredictedY As Single '12
        '  Lag in whitegate will GREATLY demonstrate the properties of these values if used.
        Public u2 As Single '16
        Public u3 As Int32 '20
        Public PredictedHeading As Single '24
        Public u4 As Int32 '28
        Public u5 As Single '32
        Public X As Single '36
        'These coords are used because it seems like a good mix between actual and predicted.
        Public Z As Single '40
        'Also note that the assinine ordering (xzy) is corrected.
        Public Y As Single '44
        Public u6 As Single '48
        Public u7 As Int32 '52
        Public Heading As Single '56
        'heading is expressed in radians. cos/sin will extract a normalized x/y coord.
        Public u8 As Int32 '60 
        Public u9 As Int32 '64
        Public X2 As Single '68
        'These are assumed to be server confirmed (actual) coords.
        Public Z2 As Single '72
        Public Y2 As Single '76
        Public u10 As Single '80
        Public u11 As Int32 '84
        Public u12 As Int32 '88
        Public u13 As Int32 '92
        Public u14 As Int32 '96
        Public u15 As Int32 '100
        Public u16 As Int32 '104
        Public u17 As Int32 '108
        Public u18 As Int32 '112
        Public ZoneID As UInt32 '116
        Public ServerID As UInt32 '120
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=24)> _
        Public DisplayName As String '124
        'player is 16 max, but npc's can be up to 24.
        Public pUnknown As Int32 '148
        Public RunSpeed As Single '152
        Public RunSpeed2 As Single '156
        Public pListNode As Int32 '160
        'a pointer to the node this record belongs to in a resource linked list. note that the data in this list contains many things not just spawn data. further down the chain is unstable.
        Public u19 As Int32 '164
        Public NPCTalking As Int32 '168
        Public Distance As Single '172
        Public u20 As Int32 '176
        Public u21 As Int32 '180
        Public Heading2 As Single '184
        Public pPetOwner As Int32 '188
        'only for permanent pets. charmed mobs do not fill this.
        Public u22 As Int32 '192
        Public HealthPercent As Byte '196
        Public u23 As Byte '197
        Public ModelType As Byte '198
        Public Race As Byte '199
        Public NPCPathingTime As Int32 '200
        Public u25 As Int32 '204
        Public u26 As Int32 '208
        Public Model As Int16 '212
        Public ModelHead As Int16 '214
        Public ModelBody As Int16 '216
        Public ModelHands As Int16 '218
        Public ModelLegs As Int16 '220
        Public ModelFeet As Int16 '222
        Public ModelMain As Int16 '224
        Public ModelSub As Int16 '226 
        Public ModelRanged As Int16 '228
        Public u27 As Int32 '230
        Public u28 As Int32 '234
        Public u29 As Int32 '238
        Public u30 As Int16 '242
        Public u31 As Int16 '244 
        Public u33 As Byte '246
        Public u34 As Byte '247
        Public u70 As Int32 '248
        'Added Feb 14th 2011 Patch; Fixed by Jetsam
        Public FlagRender As Byte '252
        Public Flags1 As Byte '253
        'I am well aware these should be combined,
        Public Flags2 As Byte '254
        '  but it is easier for future discoveries
        Public Flags3 As Byte '255
        '  as my documentation and memory dissect
        Public Flags4 As Byte '256
        '  structs have these separated (to spot
        Public Flags5 As Byte '257
        '  flag changes after performing an action).
        Public Flags6 As Byte '258
        Public Flags7 As Byte '259
        '  besides, I am not entirely sure where the
        Public Flags8 As Byte '260
        '  first flag boundary starts, and what lies
        Public Flags9 As Byte '261
        '  on a word boundary (and thus is padding).
        Public u71 As Int32 '262
        'Added Jul 13th 2011 Patch; Fixed by Zer0Blues
        Public Flags10 As Byte '266
        Public Flags11 As Byte '267
        Public Flags12 As Byte '268
        Public Flags13 As Byte '269
        Public Flags14 As Byte '270
        Public Flags15 As Byte '271
        Public Flags16 As Byte '272
        Public Flags17 As Byte '273
        Public Flags18 As Byte '274
        Public Flags19 As Byte '275
        Public Flags20 As Byte '276
        Public u35 As Byte '277
        Public u36 As Int32 '278
        Public u37 As Int32 '282
        Public NPCSpeechLoop As Int16 '286
        Public NPCSpeechFrame As Int16 '288
        Public u38 As Int32 '290
        Public u39 As Int32 '294
        Public u40 As Int16 '298
        Public RunSpeed3 As Single '300
        Public NPCWalkPos1 As Int16 '304
        Public NPCWalkPos2 As Int16 '306
        Public NPCWalkMode As Int16 '308
        Public u41 As Int16 '310
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=4)> _
        Public mou4 As String '312
        'always this. assuming an animation name
        Public FlagsCombat As UInt32 '316
        Public FlagsCombatSVR As UInt32 '320
        'im assuming this is updated after the client asks the server. there is a noticable delay between the two
        Public u42 As Int32 '324
        Public u43 As Int32 '328
        Public u44 As Int32 '332
        Public u45 As Int32 '336
        Public u46 As Int32 '340
        Public ClaimID As UInt32 '344
        'the SERVER id of the player that has claim on the mob. the claim will bounce around to whomever has the most hate. not exactly the same as /assist but it will have to do.
        Public u47 As Int32 '348
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=4)> _
        Public Animation1 As String '352
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=4)> _
        Public Animation2 As String '356
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=4)> _
        Public Animation3 As String '340
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=4)> _
        Public Animation4 As String '344
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=4)> _
        Public Animation5 As String '348
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=4)> _
        Public Animation6 As String '352
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=4)> _
        Public Animation7 As String '356
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=4)> _
        Public Animation8 As String '360
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=4)> _
        Public Animation9 As String '364
        Public AnimationTime As Int16 '368
        'guessed, but something to do with the current animation
        Public AnimationStep As Int16 '370
        'guessed, but something to do with the current animation
        Public u48 As Int16 '372
        Public u49 As Int16 '374
        Public EmoteID As UInt32 '376
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=4)> _
        Public EmoteName As String '380
        Public SpawnType As Byte '384
        Public u50 As Byte '385
        Public u51 As Int16 '386
        Public LSColorRed As Byte '388
        Public LSColorGreen As Byte '384
        Public LSColorBlue As Byte '390
        Public u52 As Byte '391
        Public u53 As Byte '392
        Public u54 As Byte '393
        Public CampaignMode As Byte '394
        'boolean value. 
        Public u55 As Byte '395
        Public u56 As Byte '396
        Public u57 As Byte '397
        Public u58 As Int16 '398
        Public u59 As Int16 '400
        Public u60 As Int16 '402
        Public u61 As Int16 '404
        Public u62 As Int16 '406
        Public u63 As Int16 '408
        Public u64 As Int16 '410
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=4)> _
        Public TalkAnimation As String '412
        Public u65 As Int32 '416
        Public u66 As Int32 '420
        Public PetID As UInt32 '424
        'the zone id of the spawn considered this spawns pet.
        Public u67 As Int16 '428
        Public u68 As Byte '430
        Public u69 As Byte '431
    End Structure

    Public Class FilterInfo
        Public Property MapFiltered As Boolean
        Public Property OverlayFiltered As Boolean
    End Class
#End Region

#Region " MEMORY PROPERTIES "
    Private _memoryObject As Memory
    ''' <summary>
    ''' The memory object used to get the data for this mob
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <XmlIgnore()>
    Public ReadOnly Property MemoryObject() As Memory
        Get
            If _memoryObject Is Nothing Then
                _memoryObject = New Memory(POL, MobBase)
            End If
            Return _memoryObject
        End Get
    End Property

    Private _mobBase As Integer
    ''' <summary>
    ''' The base address of the mobs structure
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MobBase() As Integer
        Get
            Return _mobBase
        End Get
        Set(ByVal value As Integer)
            _mobBase = value
            If Not POL Is Nothing Then
                MemoryObject.Address = value
                MobBlock = MemoryObject.GetByteArray(428)
            End If
        End Set
    End Property

    Private _pol As Process
    ''' <summary>
    ''' The POL process to use for getting the mob data
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property POL() As Process
        Get
            Return _pol
        End Get
        Set(ByVal value As Process)
            _pol = value
            If MobBase > 0 Then
                _memoryObject = New Memory(value, MobBase)
                MobBlock = MemoryObject.GetByteArray(428)
            End If
        End Set
    End Property

    Private Property MobBlock() As Byte()

    Private _lastX As Single
    ''' <summary>
    ''' Mobs last x coordinate
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LastX() As Single
        Get
            'Return _lastX
            If _dataPreLoaded Then
                Return BitConverter.ToSingle(MobBlock, MobOffsets.LastX)
            Else
                MemoryObject.Address = MobBase + MobOffsets.LastX
                Return MemoryObject.GetFloat
            End If
        End Get
        Set(ByVal value As Single)
            MemoryObject.Address = MobBase + MobOffsets.LastX
            MemoryObject.SetFloat(value)
        End Set
    End Property

    Private _lastY As Single
    ''' <summary>
    ''' Mobs last Y coordinate
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LastY() As Single
        Get
            If _dataPreLoaded Then
                Return BitConverter.ToSingle(MobBlock, MobOffsets.LastY)
            Else
                MemoryObject.Address = MobBase + MobOffsets.LastY
                Return MemoryObject.GetFloat
            End If
        End Get
        Set(ByVal value As Single)
            MemoryObject.Address = MobBase + MobOffsets.LastY
            MemoryObject.SetFloat(value)
        End Set
    End Property

    Private _lastZ As Single
    ''' <summary>
    ''' mobs last Z coordinate
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LastZ() As Single
        Get
            If _dataPreLoaded Then
                Return BitConverter.ToSingle(MobBlock, MobOffsets.LastZ)
            Else
                MemoryObject.Address = MobBase + MobOffsets.LastZ
                Return MemoryObject.GetFloat
            End If
        End Get
        Set(ByVal value As Single)
            MemoryObject.Address = MobBase + MobOffsets.LastZ
            MemoryObject.SetFloat(value)
        End Set
    End Property

    Private _lastDirection As Single
    ''' <summary>
    ''' Mobs last direction
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LastDirection() As Single
        Get
            If _dataPreLoaded Then
                Return BitConverter.ToSingle(MobBlock, MobOffsets.LastDirection)
            Else
                MemoryObject.Address = MobBase + MobOffsets.LastDirection
                Return MemoryObject.GetFloat
            End If
        End Get
        Set(ByVal value As Single)
            MemoryObject.Address = MobBase + MobOffsets.LastDirection
            MemoryObject.SetFloat(value)
        End Set
    End Property

    Private _x As Single
    ''' <summary>
    ''' Mobs current x coordinate
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property X() As Single
        Get
            If _dataPreLoaded Then
                Return BitConverter.ToSingle(MobBlock, MobOffsets.X)
            Else
                MemoryObject.Address = MobBase + MobOffsets.X
                Return MemoryObject.GetFloat
            End If
        End Get
        Set(ByVal value As Single)
            MemoryObject.Address = MobBase + MobOffsets.X
            MemoryObject.SetFloat(value)
        End Set
    End Property

    Private _y As Single
    ''' <summary>
    ''' Mobs current Y coordinate
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Y() As Single
        Get
            If _dataPreLoaded Then
                Return BitConverter.ToSingle(MobBlock, MobOffsets.Y)
            Else
                MemoryObject.Address = MobBase + MobOffsets.Y
                Return MemoryObject.GetFloat
            End If
        End Get
        Set(ByVal value As Single)
            MemoryObject.Address = MobBase + MobOffsets.Y
            MemoryObject.SetFloat(value)
        End Set
    End Property

    Private _z As Single
    ''' <summary>
    ''' Mobs current Z coordinate
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Z() As Single
        Get
            If _dataPreLoaded Then
                Return BitConverter.ToSingle(MobBlock, MobOffsets.Z)
            Else
                MemoryObject.Address = MobBase + MobOffsets.Z
                Return MemoryObject.GetFloat
            End If
        End Get
        Set(ByVal value As Single)
            MemoryObject.Address = MobBase + MobOffsets.Z
            MemoryObject.SetFloat(value)
        End Set
    End Property

    Private _direction As Single
    ''' <summary>
    ''' Mobs current Direction
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Direction() As Single
        Get
            If _dataPreLoaded Then
                Return BitConverter.ToSingle(MobBlock, MobOffsets.Direction)
            Else
                MemoryObject.Address = MobBase + MobOffsets.Direction
                Return MemoryObject.GetFloat
            End If
        End Get
        Set(ByVal value As Single)
            MemoryObject.Address = MobBase + MobOffsets.Direction
            MemoryObject.SetFloat(value)
        End Set
    End Property

    Private _id As Integer
    ''' <summary>
    ''' Mobs array ID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ID() As Integer
        Get
            If _dataPreLoaded Then
                Return BitConverter.ToInt32(MobBlock, MobOffsets.ID)
            Else
                MemoryObject.Address = MobBase + MobOffsets.ID
                Return MemoryObject.GetInt32
            End If
        End Get
        Set(ByVal value As Integer)
            MemoryObject.Address = MobBase + MobOffsets.ID
            MemoryObject.SetInt32(value)
        End Set
    End Property

    Private _serverID As Integer
    ''' <summary>
    ''' Mobs Server ID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ServerID() As Integer
        Get
            Try
                If _dataPreLoaded Then
                    Return BitConverter.ToInt32(MobBlock, MobOffsets.ServerID)
                Else
                    MemoryObject.Address = MobBase + MobOffsets.ServerID
                    Return MemoryObject.GetInt32
                End If
            Catch
                Return 0
            End Try
        End Get
        Set(ByVal value As Integer)
            MemoryObject.Address = MobBase + MobOffsets.ServerID
            MemoryObject.SetInt32(value)
        End Set
    End Property

    Private _name As String
    ''' <summary>
    ''' Mobs name
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Name() As String
        Get
            If _dataPreLoaded Then
                If ID = &H77 Then
                    _name = String.Empty
                End If
                For i As Integer = MobOffsets.Name To MobBlock.Length - 1
                    If MobBlock(i) = 0 Then
                        _name = System.Text.Encoding.Default.GetString(MobBlock, MobOffsets.Name, i - MobOffsets.Name)
                        Exit For
                    End If
                Next
                Return _name
            Else
                MemoryObject.Address = MobBase + MobOffsets.Name
                Return MemoryObject.GetName
            End If
        End Get
    End Property

    Private _warpInfo As Integer
    ''' <summary>
    ''' Pointer to mobs warp structure
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property WarpInfo() As Integer
        Get
            If _dataPreLoaded Then
                Return BitConverter.ToInt32(MobBlock, MobOffsets.WarpInfo)
            Else
                MemoryObject.Address = MobBase + MobOffsets.WarpInfo
                Return MemoryObject.GetInt32
            End If
        End Get
    End Property

    Private _distance As Single
    ''' <summary>
    ''' Mobs distance from my position
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Distance() As Single
        Get
            If _dataPreLoaded Then
                Return Math.Sqrt(BitConverter.ToSingle(MobBlock, MobOffsets.Distance))
            Else
                MemoryObject.Address = MobBase + MobOffsets.Distance
                Return Math.Sqrt(MemoryObject.GetFloat)
            End If
        End Get
    End Property

    Private _tp As Short
    ''' <summary>
    ''' Mobs current tp percent
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property TP() As Short
        Get
            If _dataPreLoaded Then
                Return BitConverter.ToInt16(MobBlock, MobOffsets.TP)
            Else
                MemoryObject.Address = MobBase + MobOffsets.TP
                Return MemoryObject.GetShort
            End If
        End Get
    End Property

    Private _hp As Byte
    ''' <summary>
    ''' Mobs current HP percent
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property HP() As Byte
        Get
            If _dataPreLoaded Then
                Return MobBlock(MobOffsets.HP)
            Else
                MemoryObject.Address = MobBase + MobOffsets.HP
                Return MemoryObject.GetByte
            End If
        End Get
    End Property

    Private _mobType As Byte
    ''' <summary>
    ''' Mobs type 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>0=PC 1=NPC 2=MOB 3=Other</remarks>
    Public ReadOnly Property MobType() As Byte
        Get
            If _dataPreLoaded Then
                Return MobBlock(MobOffsets.MobType)
            Else
                MemoryObject.Address = MobBase + MobOffsets.MobType
                Return MemoryObject.GetByte
            End If
        End Get
    End Property

    Private _race As Byte
    ''' <summary>
    ''' Mobs race
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Race() As Byte
        Get
            'Return _race
            Return MobBlock(MobOffsets.Race)
            'MemoryObject.Address = MobBase + MobOffsets.Race
            'Return MemoryObject.GetByte
        End Get
        Set(ByVal value As Byte)
            MemoryObject.Address = MobBase + MobOffsets.Race
            MemoryObject.SetByte(value)
        End Set
    End Property

    Private _attackTimer As Byte
    ''' <summary>
    ''' Mobs attack timer
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>This is the countdown until the next swing of the mob</remarks>
    Public ReadOnly Property AttackTimer() As Byte
        Get
            'Return _attackTimer
            Return MobBlock(MobOffsets.AttackTimer)
            'MemoryObject.Address = MobBase + MobOffsets.AttackTimer
            'Return MemoryObject.GetByte
        End Get
    End Property

    Private _face As Byte
    ''' <summary>
    ''' Mobs fade value
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>Not entirely sure what this is used for, more research is needed</remarks>
    Public Property Face() As Byte
        Get
            'Return _fade
            Return MobBlock(MobOffsets.Face)
            'MemoryObject.Address = MobBase + MobOffsets.Fade
            'Return MemoryObject.GetByte
        End Get
        Set(ByVal value As Byte)
            MemoryObject.Address = MobBase + MobOffsets.Face
            MemoryObject.SetByte(value)
        End Set
    End Property

    Private _hair As Short
    ''' <summary>
    ''' Mobs hair value
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Hair() As Short
        Get
            'Return _hair
            Return BitConverter.ToInt16(MobBlock, MobOffsets.Hair)
            'MemoryObject.Address = MobBase + MobOffsets.Hair
            'Return MemoryObject.GetShort()
        End Get
        Set(ByVal value As Short)
            MemoryObject.Address = MobBase + MobOffsets.Hair
            MemoryObject.SetShort(value)
        End Set
    End Property

    Private _head As Short
    ''' <summary>
    ''' Mobs head armor
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Head() As Short
        Get
            'Return _head
            Return BitConverter.ToInt16(MobBlock, MobOffsets.Head)
            'MemoryObject.Address = MobBase + MobOffsets.Head
            'Return MemoryObject.GetShort
        End Get
        Set(ByVal value As Short)
            MemoryObject.Address = MobBase + MobOffsets.Head
            MemoryObject.SetShort(value)
        End Set
    End Property

    Private _body As Short
    ''' <summary>
    ''' Mobs body armor
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Body() As Short
        Get
            'Return _body
            Return BitConverter.ToInt16(MobBlock, MobOffsets.Body)
            'MemoryObject.Address = MobBase + MobOffsets.Body
            'Return MemoryObject.GetShort
        End Get
        Set(ByVal value As Short)
            MemoryObject.Address = MobBase + MobOffsets.Body
            MemoryObject.SetShort(value)
        End Set
    End Property

    Private _hands As Short
    ''' <summary>
    ''' Mobs hand armor
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Hands() As Short
        Get
            'Return _hands
            Return BitConverter.ToInt16(MobBlock, MobOffsets.Hands)
            'MemoryObject.Address = MobBase + MobOffsets.Hands
            'Return MemoryObject.GetShort
        End Get
        Set(ByVal value As Short)
            MemoryObject.Address = MobBase + MobOffsets.Hands
            MemoryObject.SetShort(value)
        End Set
    End Property

    Private _legs As Short
    ''' <summary>
    ''' Mobs leg armor
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Legs() As Short
        Get
            'Return _legs
            Return BitConverter.ToInt16(MobBlock, MobOffsets.Legs)
            'MemoryObject.Address = MobBase + MobOffsets.Legs
            'Return MemoryObject.GetShort
        End Get
        Set(ByVal value As Short)
            MemoryObject.Address = MobBase + MobOffsets.Legs
            MemoryObject.SetShort(value)
        End Set
    End Property

    Private _feet As Short
    ''' <summary>
    ''' Mobs feet armor
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Feet() As Short
        Get
            'Return _feet
            Return BitConverter.ToInt16(MobBlock, MobOffsets.Feet)
            'MemoryObject.Address = MobBase + MobOffsets.Feet
            'Return MemoryObject.GetShort
        End Get
        Set(ByVal value As Short)
            MemoryObject.Address = MobBase + MobOffsets.Feet
            MemoryObject.SetShort(value)
        End Set
    End Property

    Private _mainWeapon As Short
    ''' <summary>
    ''' Mobs main weapon
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MainWeapon() As Short
        Get
            'Return _mainWeapon
            Return BitConverter.ToInt16(MobBlock, MobOffsets.MainWeapon)
            'MemoryObject.Address = MobBase + MobOffsets.MainWeapon
            'Return MemoryObject.GetShort
        End Get
        Set(ByVal value As Short)
            MemoryObject.Address = MobBase + MobOffsets.MainWeapon
            MemoryObject.SetShort(value)
        End Set
    End Property

    Private _subWeapon As Short
    ''' <summary>
    ''' Mobs sub weapon
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SubWeapon() As Short
        Get
            'Return _subWeapon
            Return BitConverter.ToInt16(MobBlock, MobOffsets.SubWeapon)
            'MemoryObject.Address = MobBase + MobOffsets.SubWeapon
            'Return MemoryObject.GetShort
        End Get
        Set(ByVal value As Short)
            MemoryObject.Address = MobBase + MobOffsets.SubWeapon
            MemoryObject.SetShort(value)
        End Set
    End Property

    Private _pIcon As Byte
    ''' <summary>
    ''' Mobs player icon
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PIcon() As Byte
        Get
            If _dataPreLoaded Then
                Return MobBlock(MobOffsets.PIcon)
            Else
                MemoryObject.Address = MobBase + MobOffsets.PIcon
                Return MemoryObject.GetByte
            End If
        End Get
        Set(ByVal value As Byte)
            MemoryObject.Address = MobBase + MobOffsets.PIcon
            MemoryObject.SetByte(value)
        End Set
    End Property

    Private _gIcon As Short
    ''' <summary>
    ''' Mobs GM Icon
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GIcon() As Short
        Get
            'Return _gIcon
            Return BitConverter.ToInt16(MobBlock, MobOffsets.GIcon)
            'MemoryObject.Address = MobBase + MobOffsets.GIcon
            'Return MemoryObject.GetShort
        End Get
        Set(ByVal value As Short)
            MemoryObject.Address = MobBase + MobOffsets.GIcon
            MemoryObject.SetShort(value)
        End Set
    End Property

    Private _speed As Single
    ''' <summary>
    ''' Mobs speed
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Speed() As Single
        Get
            'Return _speed
            Return BitConverter.ToSingle(MobBlock, MobOffsets.Speed)
            'MemoryObject.Address = MobBase + MobOffsets.Speed
            'Return MemoryObject.GetFloat
        End Get
        Set(ByVal value As Single)
            MemoryObject.Address = MobBase + MobOffsets.Speed
            MemoryObject.SetFloat(value)
        End Set
    End Property

    Private _status As Integer
    ''' <summary>
    ''' Mobs status
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Status() As Integer
        Get
            'Return _status
            Return BitConverter.ToInt32(MobBlock, MobOffsets.Status)
            'MemoryObject.Address = MobBase + MobOffsets.Status
            'Return MemoryObject.GetInt32
        End Get
        Set(ByVal value As Integer)
            MemoryObject.Address = MobBase + MobOffsets.Status
            MemoryObject.SetInt32(value)
        End Set
    End Property

    Private _status2 As Integer
    ''' <summary>
    ''' Mobs status2
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Status2() As Integer
        Get
            'Return _status2
            'MemoryObject.Address = MobBase + MobOffsets.Status2
            'Return MemoryObject.GetInt32
        End Get
        Set(ByVal value As Integer)
            MemoryObject.Address = MobBase + MobOffsets.Status2
            MemoryObject.SetInt32(value)
        End Set
    End Property

    Private _claimedBy As Integer
    ''' <summary>
    ''' ServerId of the player that has the mob claimed
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property ClaimedBy() As Integer
        Get
            If _dataPreLoaded Then
                Return BitConverter.ToInt32(MobBlock, MobOffsets.ClaimedBy)
            Else
                MemoryObject.Address = MobBase + MobOffsets.ClaimedBy
                Return MemoryObject.GetInt32
            End If

        End Get
    End Property

    Public ReadOnly Property SpawnType As SpawnTypes
        Get
            If _dataPreLoaded Then
                Return [Enum].Parse(GetType(SpawnTypes), MobBlock(MobOffsets.SpawnType))
            Else
                MemoryObject.Address = MobBase + MobOffsets.SpawnType
                Return [Enum].Parse(GetType(SpawnTypes), MemoryObject.GetByte())
            End If
        End Get
    End Property

    Private _petIndex As Short
    ''' <summary>
    ''' Mobs pet index
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property PetIndex() As Short
        Get
            If _dataPreLoaded Then
                Return BitConverter.ToInt16(MobBlock, MobOffsets.PetIndex)
            Else
                MemoryObject.Address = MobBase + MobOffsets.PetIndex
                Return MemoryObject.GetShort
            End If
        End Get
    End Property

    Public ReadOnly Property PCTarget As Short
        Get
            If _dataPreLoaded Then
                Return BitConverter.ToInt16(MobBlock, MobOffsets.PCTarget)
            Else
                MemoryObject.Address = MobBase + MobOffsets.PCTarget
                Return MemoryObject.GetShort
            End If
        End Get
    End Property


#End Region

#Region " MAP PROPERTIES "
    ''' <summary>
    ''' The X Distance of the mob in relation to my position on the radar
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property XDistance() As Single
    ''' <summary>
    ''' The Y Distance of the mob in relation to my position on the radar
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property YDistance() As Single
    ''' <summary>
    ''' The Z Distance of the mob in relation to my position on the radar
    ''' </summary>
    ''' <remarks></remarks>
    Public Property ZDistance() As Single
    ''' <summary>
    ''' The mobs degree rotation on the radar from 0 and in relation to my direction
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Degrees() As Single
    ''' <summary>
    ''' The radius of the mobs rotation circle on the radar
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Radius() As Single
    ''' <summary>
    ''' The mobs X position on the radar
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MapX() As Single
    ''' <summary>
    ''' the mobs Y position on the radar
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MapY() As Single
#End Region

#Region " DISPLAY PROPERTIES "
    Public Property MobIsDead As Boolean
    Private _filters As FilterInfo
    Public ReadOnly Property Filters As FilterInfo
        Get
            If _filters Is Nothing Then
                _filters = New FilterInfo With {.MapFiltered = False, .OverlayFiltered = False}
            End If
            Return _filters
        End Get
    End Property

    Public Property OverlayFiltered As Boolean
    Public Property MapFiltered As Boolean

#End Region

#Region " PRIVATE METHODS "
    Private Sub LoadData()
        'If _isPacket Then
        '    _x = _mobPacket.X
        '    _y = _mobPacket.Y
        '    _z = _mobPacket.Z
        '    _direction = _mobPacket.Direction
        '    _hp = _mobPacket.HP
        '    _id = _mobPacket.ID
        '    _serverID = _mobPacket.ServerID
        '    _mobType = MobTypes.NPC
        '    _name = _zones.GetMobName(_mapID, _mobPacket.ServerID)
        '    _warpInfo = 100
        'Else
        _lastX = BitConverter.ToSingle(MobBlock, MobOffsets.LastX)
        _lastY = BitConverter.ToSingle(MobBlock, MobOffsets.LastY)
        _lastZ = BitConverter.ToSingle(MobBlock, MobOffsets.LastZ)
        _lastDirection = BitConverter.ToSingle(MobBlock, MobOffsets.LastDirection)
        _x = BitConverter.ToSingle(MobBlock, MobOffsets.X)
        _y = BitConverter.ToSingle(MobBlock, MobOffsets.Y)
        _z = BitConverter.ToSingle(MobBlock, MobOffsets.Z)
        _direction = BitConverter.ToSingle(MobBlock, MobOffsets.Direction)
        _id = BitConverter.ToInt32(MobBlock, MobOffsets.ID)
        _serverID = BitConverter.ToInt32(MobBlock, MobOffsets.ServerID)
        For i As Integer = MobOffsets.Name To MobBlock.Length - 1
            If MobBlock(i) = 0 Then
                _name = System.Text.Encoding.Default.GetString(MobBlock, MobOffsets.Name, i - MobOffsets.Name)
                Exit For
            End If
        Next
        _warpInfo = BitConverter.ToInt32(MobBlock, MobOffsets.WarpInfo)
        _distance = Math.Sqrt(BitConverter.ToSingle(MobBlock, MobOffsets.Distance))
        _tp = BitConverter.ToInt16(MobBlock, MobOffsets.TP)
        _hp = MobBlock(MobOffsets.HP)
        If MobBlock(MobOffsets.MobType) > 0 Then
            _mobType = MobTypes.NPC
        Else
            _mobType = MobTypes.PC
        End If
        _race = MobBlock(MobOffsets.Race)
        _attackTimer = MobBlock(MobOffsets.AttackTimer)
        _face = MobBlock(MobOffsets.Face)
        _hair = BitConverter.ToInt16(MobBlock, MobOffsets.Hair)
        _head = BitConverter.ToInt16(MobBlock, MobOffsets.Head)
        _body = BitConverter.ToInt16(MobBlock, MobOffsets.Body)
        _hands = BitConverter.ToInt16(MobBlock, MobOffsets.Hands)
        _legs = BitConverter.ToInt16(MobBlock, MobOffsets.Legs)
        _feet = BitConverter.ToInt16(MobBlock, MobOffsets.Feet)
        _mainWeapon = BitConverter.ToInt16(MobBlock, MobOffsets.MainWeapon)
        _subWeapon = BitConverter.ToInt16(MobBlock, MobOffsets.SubWeapon)
        _pIcon = MobBlock(MobOffsets.PIcon)
        _gIcon = BitConverter.ToInt16(MobBlock, MobOffsets.GIcon)
        _speed = BitConverter.ToSingle(MobBlock, MobOffsets.Speed)
        _status = BitConverter.ToInt32(MobBlock, MobOffsets.Status)
        _status2 = BitConverter.ToInt32(MobBlock, MobOffsets.Status2)
        _claimedBy = BitConverter.ToInt32(MobBlock, MobOffsets.ClaimedBy)
        _petIndex = BitConverter.ToInt16(MobBlock, MobOffsets.PetIndex)
        'End If
    End Sub

    Private Function GetDistance(ByVal ReferencePoint As Point, ByVal MobLocation As Point) As Single
        Return Math.Sqrt((MobLocation.X - ReferencePoint.X) ^ 2 + (MobLocation.Y - ReferencePoint.Y) ^ 2)
    End Function
#End Region

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                If Not Me.MobBlock Is Nothing Then
                    Erase MobBlock
                End If
            End If
            MobBlock = Nothing

        End If
        Me.disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class
