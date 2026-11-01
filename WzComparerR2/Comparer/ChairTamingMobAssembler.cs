using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WzComparerR2.AvatarCommon;
using WzComparerR2.CharaSim;
using WzComparerR2.Common;
using WzComparerR2.Config;
using WzComparerR2.PluginBase;
using WzComparerR2.WzLib;

namespace WzComparerR2.Comparer
{
    public partial class EasyComparer
    {
        private const string ChairExampleFolderName = "ChairExamples";
        private const string ChairExampleLogName = "chair_export.log";
        private const string TamingMobExampleFolderName = "TamingMobExamples";
        private const string TamingMobExampleLogName = "tamingmob_export.log";
        private const int AnimSlotBody = -1;
        private const int AnimSlotFace = -2;
        private const int AnimSlotTaming = -3;

        /// <summary>
        /// Actions rendered for the taming mob avatar examples (action names on the TamingMob side).
        /// </summary>
        private static readonly string[] TamingMobRenderActions = { "sit", "stand", "walk1", "prone", "jump", "ladder", "rope", "fly" };

        /// <summary>
        /// Maps a TamingMob action name to the action name of the character body.
        /// </summary>
        private static readonly Dictionary<string, string> TamingMobBodyActions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "stand1", "stand1" },
            { "stand2", "stand1" },
            { "walk1", "walk1" },
            { "walk2", "walk1" },
            { "prone", "prone" },
            { "jump", "jump" },
            { "ladder", "ladder" },
            { "rope", "rope" },
            { "sit", "sit" },
            { "fly", "fly" },
            { "tired", "tired" },
        };
        private Dictionary<string, Dictionary<int, Wz_Node>> itemNodeCache = new Dictionary<string, Dictionary<int, Wz_Node>>();

        /// <summary>
        /// For every chair collected in OutputChairIDs, renders the avatar sitting on it
        /// and writes it to <c>ChairExamples/chair_&lt;ID&gt;_&lt;name&gt;</c>.
        /// </summary>
        private void SaveChairExamples(string outputDir)
        {
            if (!this.OutputChairAndRidingMount)
            {
                return;
            }
            if (this.OutputChairIDs == null || this.OutputChairIDs.Count <= 0)
            {
                return;
            }

            string chairExamplePath = Path.Combine(outputDir, ChairExampleFolderName);
            Directory.CreateDirectory(chairExamplePath);

            var config = ImageHandlerConfig.Default;
            var stringLinker = this.CreateChairStringLinker();
            var log = new StringBuilder();
            log.AppendLine($"chair examples: {this.OutputChairIDs.Count} item(s)");
            log.AppendLine($"avatar code: {this.AvatarCode}");
            log.AppendLine($"zmap: {(this.ResolveWzNode("Base\\zmap.img") != null ? "ok" : "missing")}");
            log.AppendLine($"body(00002000.img): {(this.ResolveWzNode("Character\\00002000.img") != null ? "ok" : "missing")}");
            log.AppendLine();

            int count = 0;
            int allCount = this.OutputChairIDs.Count;

            foreach (string chairID in this.OutputChairIDs.OrderBy(id => id, StringComparer.Ordinal))
            {
                try
                {
                    StateInfo = string.Format("{0}/{1} 椅子: {2}", ++count, allCount, chairID);
                    StateDetail = "Chair 角色外觀範例輸出中...";

                    string result = this.SaveChairExample(chairID, chairExamplePath, config, stringLinker);
                    log.AppendLine($"[{chairID}] {result}");
                }
                catch (Exception ex)
                {
                    log.AppendLine($"[{chairID}] ERROR {ex.GetType().Name}: {ex.Message}");
                    log.AppendLine(ex.StackTrace);
                    FailToExportTooltips["Chair Example: " + chairID] = ex.Message + ex.StackTrace;
                }
            }

            try
            {
                File.WriteAllText(Path.Combine(chairExamplePath, ChairExampleLogName), log.ToString(), Encoding.UTF8);
            }
            catch
            {
            }

            this.OutputChairIDs.Clear();
        }

        /// <summary>
        /// For every changed taming mob, renders the avatar riding it for each action
        /// and writes it to <c>TamingMobExamples/tamingmob_&lt;ID&gt;_&lt;name&gt;_&lt;action&gt;</c>.
        /// </summary>
        private void SaveTamingMobExamples(string outputDir)
        {
            if (!this.OutputChairAndRidingMount)
            {
                return;
            }
            bool hasTamingMobChanges = this.OutputTamingMobIDs != null && this.OutputTamingMobIDs.Count > 0;
            bool hasItemChanges = this.DiffItemIDs != null && this.DiffItemIDs.Count > 0;
            if (!hasTamingMobChanges && !hasItemChanges)
            {
                return;
            }

            string tamingMobExamplePath = Path.Combine(outputDir, TamingMobExampleFolderName);
            Directory.CreateDirectory(tamingMobExamplePath);

            var config = ImageHandlerConfig.Default;
            var stringLinker = this.CreateChairStringLinker();
            var log = new StringBuilder();
            var tamingMobItemIDs = new Dictionary<string, string>(StringComparer.Ordinal);
            var tamingMobIDs = this.CollectRenderableTamingMobIDs(log, tamingMobItemIDs);

            log.Insert(0, string.Format("taming mob examples: {0} id(s){1}avatar code: {2}{1}{1}",
                tamingMobIDs.Count, Environment.NewLine, this.AvatarCode));

            int count = 0;
            int allCount = tamingMobIDs.Count;

            foreach (string tamingMobID in tamingMobIDs)
            {
                try
                {
                    StateInfo = string.Format("{0}/{1} 騎乘座騎: {2}", ++count, allCount, tamingMobID);
                    StateDetail = "TamingMob 角色外觀範例輸出中...";

                    string result = this.SaveTamingMobExample(tamingMobID, tamingMobExamplePath, config, stringLinker, tamingMobItemIDs);
                    log.AppendLine($"[{tamingMobID}] {result}");
                }
                catch (Exception ex)
                {
                    log.AppendLine($"[{tamingMobID}] ERROR {ex.GetType().Name}: {ex.Message}");
                    log.AppendLine(ex.StackTrace);
                    FailToExportTooltips["TamingMob Example: " + tamingMobID] = ex.Message + ex.StackTrace;
                }
            }

            try
            {
                File.WriteAllText(Path.Combine(tamingMobExamplePath, TamingMobExampleLogName), log.ToString(), Encoding.UTF8);
            }
            catch
            {
            }

            this.OutputTamingMobIDs.Clear();
            this.DiffItemIDs.Clear();
        }

        /// <summary>
        /// Collects the taming mob IDs to render.
        /// <br/>IDs that only appear in info\tamingMob of a chair (0301*) are excluded, since the chair examples already cover them.
        /// </summary>
        private SortedSet<string> CollectRenderableTamingMobIDs(StringBuilder log, Dictionary<string, string> itemIDByTamingMob)
        {
            var renderIDs = new SortedSet<string>(StringComparer.Ordinal);
            var chairDerivedIDs = new SortedSet<string>(StringComparer.Ordinal);

            // changes of the taming mob images themselves (Character\TamingMob\*.img)
            if (this.OutputTamingMobIDs != null)
            {
                renderIDs.UnionWith(this.OutputTamingMobIDs);
            }

            // info\tamingMob of the changed items
            if (this.DiffItemIDs != null)
            {
                foreach (string itemID in this.DiffItemIDs)
                {
                    int itemIDValue;
                    if (!int.TryParse(itemID, out itemIDValue))
                    {
                        continue;
                    }

                    Wz_Node itemNode = this.FindItemNode(itemIDValue);
                    if (itemNode == null)
                    {
                        continue;
                    }

                    int tamingMobID = itemNode.FindNodeByPath("info\\tamingMob").GetValueEx<int>(0);
                    if (tamingMobID == 0)
                    {
                        tamingMobID = itemNode.FindNodeByPath("info\\customChair\\self\\tamingMob").GetValueEx<int>(0);
                    }
                    if (tamingMobID == 0)
                    {
                        continue;
                    }

                    string tamingMobIDText = tamingMobID.ToString();
                    if (itemID.StartsWith("0301")) // from a chair
                    {
                        chairDerivedIDs.Add(tamingMobIDText);
                    }
                    else
                    {
                        renderIDs.Add(tamingMobIDText);
                        if (!itemIDByTamingMob.ContainsKey(tamingMobIDText))
                        {
                            itemIDByTamingMob[tamingMobIDText] = itemID;
                        }
                    }
                }
            }

            var excludedIDs = chairDerivedIDs.Where(id => !renderIDs.Contains(id)).ToList();
            if (excludedIDs.Count > 0)
            {
                log.AppendLine($"chair-derived taming mob excluded: {string.Join(", ", excludedIDs)}");
            }

            return renderIDs;
        }

        /// <summary>
        /// Writes the avatar examples of a single taming mob for every action and returns a summary of the result.
        /// </summary>
        private string SaveTamingMobExample(string tamingMobID, string outputPath, ImageHandlerConfig config, StringLinker stringLinker, Dictionary<string, string> itemIDByTamingMob)
        {
            int tamingMobIDValue;
            if (!int.TryParse(tamingMobID, out tamingMobIDValue))
            {
                return "SKIP invalid id";
            }

            Wz_Node tamingMobNode = this.FindTamingMobNode(tamingMobIDValue);
            if (tamingMobNode == null)
            {
                return "SKIP taming mob node not found";
            }

            var canvas = new AvatarCanvas();
            try
            {
                var notes = new List<string>();
                if (!canvas.LoadZ())
                {
                    notes.Add("zmap missing");
                }
                if (!canvas.LoadActions())
                {
                    notes.Add("body actions missing");
                }

                canvas.ActionName = "stand1";
                canvas.EmotionName = "default";
                canvas.TamingActionName = null;

                int loadedParts = this.LoadAvatarCodeIntoCanvas(canvas, this.AvatarCode, notes);
                if (canvas.Body == null)
                {
                    notes.Add("avatar body missing");
                }
                if (loadedParts <= 0)
                {
                    notes.Add("avatar code produced no part");
                }

                if (canvas.AddTamingPart(tamingMobNode,
                        BitmapOrigin.CreateFromNode(tamingMobNode.FindNodeByPath("info\\icon"), PluginManager.FindWz),
                        tamingMobIDValue,
                        false) == null)
                {
                    return "SKIP taming mob has no info node";
                }

                canvas.LoadEmotions();
                canvas.LoadAllEffects();

                string tamingMobName = GetTamingMobName(stringLinker, tamingMobIDValue, itemIDByTamingMob);
                var mountActions = GetTamingMobActionsToRender(canvas, notes);
                var results = new List<string>();
                int renderedActions = 0;

                foreach (string mountAction in mountActions)
                {
                    if (canvas.GetTamingFrames(mountAction).Length <= 0)
                    {
                        results.Add(mountAction + ":none");
                        continue;
                    }

                    string bodyAction = ResolveTamingMobBodyAction(canvas, tamingMobNode, mountAction);
                    string emotionName = tamingMobNode.FindNodeByPath("characterEmotion\\" + mountAction).GetValueEx<string>(null);
                    if (string.IsNullOrEmpty(emotionName) || canvas.GetFaceFrames(emotionName).Length <= 0)
                    {
                        emotionName = "default";
                    }

                    canvas.ActionName = bodyAction;
                    canvas.EmotionName = emotionName;
                    canvas.TamingActionName = mountAction;

                    var tracks = BuildTamingMobAnimTracks(canvas, bodyAction, mountAction);
                    if (tracks.Count <= 0)
                    {
                        results.Add(mountAction + ":notrack");
                        continue;
                    }

                    string fileName = Path.Combine(outputPath, string.Format("tamingmob_{0}_{1}_{2}",
                        tamingMobID, RemoveInvalidFileNameChars(tamingMobName), mountAction));
                    int frameCount = WriteAvatarAnimation(canvas, tracks, config, fileName, out _);
                    if (frameCount <= 0)
                    {
                        results.Add(mountAction + ":empty");
                        continue;
                    }

                    renderedActions++;
                    results.Add($"{mountAction}:{frameCount}f");
                }

                string summary = string.Join(", ", results);
                string extra = notes.Count > 0 ? " (" + string.Join(", ", notes) + ")" : "";

                if (renderedActions <= 0)
                {
                    return $"SKIP no renderable action [{summary}] available=[{string.Join(", ", canvas.TamingActions)}]{extra}";
                }

                return $"OK {renderedActions}/{mountActions.Count} action(s) [{summary}] {tamingMobName}{extra}";
            }
            finally
            {
                canvas.ClearSkinCache();
            }
        }

        /// <summary>
        /// Decides which actions to render.
        /// <br/>Normally stand/walk/prone/jump/ladder/rope are rendered,
        /// and mounts that only provide sit (vehicles) fall back to rendering sit instead.
        /// </summary>
        private static List<string> GetTamingMobActionsToRender(AvatarCanvas canvas, List<string> notes)
        {
            var actions = TamingMobRenderActions.Where(a => canvas.GetTamingFrames(a).Length > 0).ToList();
            if (actions.Count > 0)
            {
                return actions;
            }

            if (canvas.GetTamingFrames("sit").Length > 0)
            {
                notes.Add("only sit available");
                return new List<string> { "sit" };
            }

            return new List<string>(TamingMobRenderActions);
        }

        /// <summary>
        /// Resolves the character body action name from a TamingMob action name.
        /// </summary>
        private static string ResolveTamingMobBodyAction(AvatarCanvas canvas, Wz_Node tamingMobNode, string mountAction)
        {
            string bodyAction = tamingMobNode.FindNodeByPath("characterAction\\" + mountAction).GetValueEx<string>(null);
            if (string.IsNullOrEmpty(bodyAction))
            {
                TamingMobBodyActions.TryGetValue(mountAction, out bodyAction);
            }

            if (!string.IsNullOrEmpty(bodyAction) && canvas.GetActionFrames(bodyAction).Length > 0)
            {
                return bodyAction;
            }

            foreach (string candidate in new[] { mountAction, "stand1", "stand" })
            {
                if (!string.IsNullOrEmpty(candidate) && canvas.GetActionFrames(candidate).Length > 0)
                {
                    return candidate;
                }
            }

            return canvas.Actions.Count > 0 ? canvas.Actions[0].Name : "stand1";
        }

        private static List<AvatarAnimTrack> BuildTamingMobAnimTracks(AvatarCanvas canvas, string bodyAction, string mountAction)
        {
            var tracks = new List<AvatarAnimTrack>();

            ActionFrame[] bodyFrames = canvas.GetActionFrames(bodyAction);
            if (bodyFrames.Length > 0)
            {
                tracks.Add(new AvatarAnimTrack(AnimSlotBody, bodyFrames));
            }

            if (!string.IsNullOrEmpty(canvas.EmotionName))
            {
                ActionFrame[] faceFrames = canvas.GetFaceFrames(canvas.EmotionName);
                if (faceFrames.Length > 0)
                {
                    tracks.Add(new AvatarAnimTrack(AnimSlotFace, new[] { faceFrames[0] }));
                }
            }

            ActionFrame[] tamingFrames = canvas.GetTamingFrames(mountAction);
            if (tamingFrames.Length > 0)
            {
                tracks.Add(new AvatarAnimTrack(AnimSlotTaming, tamingFrames));
            }

            return tracks;
        }

        private Wz_Node FindTamingMobNode(int tamingMobID)
        {
            string imgName = tamingMobID.ToString("D8") + ".img";
            foreach (string path in new[] { @"Character\TamingMob\" + imgName, @"TamingMob\" + imgName })
            {
                Wz_Node node = this.ResolveWzNode(path);
                if (node != null && node.FindNodeByPath("info") != null)
                {
                    return node;
                }
            }

            // some WZ files do not zero pad the node name to 8 digits, so match on the numeric id
            Wz_Node tamingMobRoot = this.ResolveWzNode(@"Character\TamingMob");
            if (tamingMobRoot != null)
            {
                foreach (var child in tamingMobRoot.Nodes)
                {
                    string text = child.Text.EndsWith(".img", StringComparison.OrdinalIgnoreCase)
                        ? child.Text.Substring(0, child.Text.Length - 4)
                        : child.Text;

                    int childID;
                    if (!int.TryParse(text, out childID) || childID != tamingMobID)
                    {
                        continue;
                    }

                    Wz_Node extracted = ExtractImageNode(child);
                    if (extracted != null)
                    {
                        return extracted;
                    }
                }
            }

            return null;
        }

        private static string GetTamingMobName(StringLinker stringLinker, int tamingMobID, Dictionary<string, string> itemIDByTamingMob)
        {
            StringResult sr;
            if (stringLinker != null && stringLinker.StringEqp.TryGetValue(tamingMobID, out sr) && !string.IsNullOrEmpty(sr.Name))
            {
                return sr.Name;
            }

            // when the id came from a riding item (0520xxx etc.), use the item name
            string itemIDText;
            int itemID;
            if (itemIDByTamingMob != null
                && itemIDByTamingMob.TryGetValue(tamingMobID.ToString(), out itemIDText)
                && int.TryParse(itemIDText, out itemID)
                && stringLinker != null
                && stringLinker.StringItem.TryGetValue(itemID, out sr)
                && !string.IsNullOrEmpty(sr.Name))
            {
                return sr.Name;
            }

            if (stringLinker != null && stringLinker.StringItem.TryGetValue(tamingMobID, out sr) && !string.IsNullOrEmpty(sr.Name))
            {
                return sr.Name;
            }

            return "未知的騎乘座騎";
        }

        /// <summary>
        /// Writes the avatar example of a single chair and returns a summary of the result.
        /// </summary>
        private string SaveChairExample(string chairID, string outputPath, ImageHandlerConfig config, StringLinker stringLinker)
        {
            int chairIDValue;
            if (!int.TryParse(chairID, out chairIDValue))
            {
                return "SKIP invalid id";
            }

            Wz_Node chairNode = this.FindChairItemNode(chairIDValue);
            if (chairNode == null)
            {
                return "SKIP chair node not found (searched Item\\Install\\*.img by id)";
            }

            var canvas = new AvatarCanvas();
            try
            {
                var notes = new List<string>();
                if (!canvas.LoadZ())
                {
                    notes.Add("zmap missing");
                }
                if (!canvas.LoadActions())
                {
                    notes.Add("body actions missing");
                }

                canvas.ActionName = "stand1";
                canvas.EmotionName = "default";
                canvas.TamingActionName = null;

                int loadedParts = this.LoadAvatarCodeIntoCanvas(canvas, this.AvatarCode, notes);
                if (canvas.Body == null)
                {
                    notes.Add("avatar body missing");
                }
                if (loadedParts <= 0)
                {
                    notes.Add("avatar code produced no part");
                }

                Wz_Vector bodyRelMove = chairNode.FindNodeByPath("info\\bodyRelMove").GetValueEx<Wz_Vector>(null);
                bool forceAction = bodyRelMove != null && chairNode.FindNodeByPath("info\\sitAction").GetValueEx<string>(null) != null;

                // riding chair (chair that references a TamingMob): add the mount the same way as AvatarForm.OnSelectedNode2Changed
                int tamingMobID = chairNode.FindNodeByPath("info\\tamingMob").GetValueEx<int>(0);
                if (tamingMobID == 0)
                {
                    tamingMobID = chairNode.FindNodeByPath("info\\customChair\\self\\tamingMob").GetValueEx<int>(0);
                }
                if (tamingMobID != 0)
                {
                    Wz_Node tamingMobNode = this.ResolveWzNode(string.Format(@"Character\TamingMob\{0:D8}.img", tamingMobID));
                    if (tamingMobNode != null)
                    {
                        Wz_Vector tamingBrm = chairNode.FindNodeByPath("info\\group\\sit\\0\\bodyRelMove").GetValueEx<Wz_Vector>(null);
                        canvas.AddTamingPart(tamingMobNode,
                            BitmapOrigin.CreateFromNode(tamingMobNode.FindNodeByPath("info\\icon"), PluginManager.FindWz),
                            tamingMobID,
                            false,
                            tamingBrm);
                        notes.Add("taming mob " + tamingMobID);
                    }
                    else
                    {
                        notes.Add("taming mob missing: " + tamingMobID);
                    }
                }

                ChairPart chairPart = canvas.AddChairPart(
                    chairNode,
                    BitmapOrigin.CreateFromNode(chairNode.FindNodeByPath("info\\icon"), PluginManager.FindWz),
                    chairIDValue,
                    bodyRelMove,
                    forceAction);
                if (chairPart == null)
                {
                    return "SKIP chair has no info node";
                }

                canvas.LoadEmotions();
                canvas.LoadAllEffects();

                string actionName = GetChairActionName(canvas, chairPart, notes);
                int fixFrameIdx = chairPart.Node.FindNodeByPath("info\\fixFrameIdx").GetValueEx<int>(-1);
                if (fixFrameIdx < 0)
                {
                    fixFrameIdx = chairPart.Node.FindNodeByPath("info\\fixActionFrame").GetValueEx<int>(-1);
                }

                int sitEmotion = chairPart.Node.FindNodeByPath("info\\sitEmotion").GetValueEx<int>(0);
                if (sitEmotion > 0 && sitEmotion < AvatarCanvas.EmotionTable.Count)
                {
                    canvas.EmotionName = AvatarCanvas.EmotionTable[sitEmotion];
                }

                canvas.ActionName = actionName;

                var tracks = BuildAvatarAnimTracks(canvas, actionName, fixFrameIdx, notes);
                if (tracks.Count <= 0)
                {
                    return "SKIP no animation track (" + string.Join(", ", notes) + ")";
                }

                string chairName = GetChairName(stringLinker, chairIDValue);
                string fileName = Path.Combine(outputPath, string.Format("chair_{0}_{1}",
                    chairID, RemoveInvalidFileNameChars(chairName)));

                int frameCount = WriteAvatarAnimation(canvas, tracks, config, fileName, out string extension);
                if (frameCount <= 0)
                {
                    return "SKIP no frame rendered (" + string.Join(", ", notes) + ")";
                }

                string extra = notes.Count > 0 ? " (" + string.Join(", ", notes) + ")" : "";
                return $"OK {frameCount} frame(s), {tracks.Count} track(s) -> {Path.GetFileName(fileName)}{extension}{extra}";
            }
            finally
            {
                canvas.ClearSkinCache();
            }
        }

        /// <summary>
        /// Writes the frame sequence to a file using the current image output settings (encoder).
        /// </summary>
        /// <returns>Number of frames written.</returns>
        private static int WriteAvatarAnimation(AvatarCanvas canvas, List<AvatarAnimTrack> tracks, ImageHandlerConfig config, string fileNameWithoutExtension, out string extension)
        {
            extension = null;
            if (tracks == null || tracks.Count <= 0)
            {
                return 0;
            }

            using (var encoder = AnimateEncoderFactory.CreateEncoder(config))
            {
                var cap = encoder.Compatibility;
                extension = cap.DefaultExtension;
                string fileName = fileNameWithoutExtension + cap.DefaultExtension;

                var frames = RenderAvatarAnimFrames(canvas, tracks, config, cap.IsFixedFrameRate);
                try
                {
                    if (frames.Count <= 0)
                    {
                        return 0;
                    }

                    Rectangle canvasRect = frames
                        .Select(f => new Rectangle(-f.Origin.X, -f.Origin.Y, f.Bitmap.Width, f.Bitmap.Height))
                        .Aggregate((rect1, rect2) =>
                        {
                            int left = Math.Min(rect1.X, rect2.X);
                            int top = Math.Min(rect1.Y, rect2.Y);
                            int right = Math.Max(rect1.Right, rect2.Right);
                            int bottom = Math.Max(rect1.Bottom, rect2.Bottom);
                            return new Rectangle(left, top, right - left, bottom - top);
                        });

                    encoder.Init(fileName, canvasRect.Width, canvasRect.Height);

                    using (var backgroundBrush = CreateExampleBackgroundBrush(config))
                    {
                        foreach (GifFrame gifFrame in frames)
                        {
                            using (var bmp = new Bitmap(canvasRect.Width, canvasRect.Height, PixelFormat.Format32bppArgb))
                            {
                                using (var g = Graphics.FromImage(bmp))
                                {
                                    if (backgroundBrush != null)
                                    {
                                        g.FillRectangle(backgroundBrush, 0, 0, bmp.Width, bmp.Height);
                                    }
                                    ((IGifFrame)gifFrame).Draw(g, canvasRect);
                                }
                                encoder.AppendFrame(bmp, Math.Max(cap.MinFrameDelay, gifFrame.Delay));
                            }
                        }
                    }

                    return frames.Count;
                }
                finally
                {
                    foreach (var gifFrame in frames)
                    {
                        gifFrame.Bitmap?.Dispose();
                    }
                }
            }
        }

        private static string GetChairActionName(AvatarCanvas canvas, ChairPart chairPart, List<string> notes)
        {
            string actionName;
            Wz_Node sitActionNode = chairPart.Node.FindNodeByPath("info\\sitAction");
            if (sitActionNode != null)
            {
                actionName = sitActionNode.GetValueEx<string>("sit");
            }
            else if (chairPart.Node.FindNodeByPath("info\\removeBody").GetValueEx<int>(0) != 0)
            {
                actionName = "hideBody";
            }
            else if (chairPart.CustomChairType == CustomChairType.ScaleAvatarChair)
            {
                actionName = "stand1";
            }
            else
            {
                actionName = "sit";
            }

            foreach (string candidate in new[] { actionName, "sit", "stand1" })
            {
                if (!string.IsNullOrEmpty(candidate) && canvas.GetActionFrames(candidate).Length > 0)
                {
                    return candidate;
                }
            }

            notes.Add("no body action frame");
            return "stand1";
        }

        /// <summary>
        /// Builds the frame sequence of every layer played in the avatar example.
        /// </summary>
        private static List<AvatarAnimTrack> BuildAvatarAnimTracks(AvatarCanvas canvas, string actionName, int fixFrameIdx, List<string> notes)
        {
            var tracks = new List<AvatarAnimTrack>();

            // character
            ActionFrame[] bodyFrames = canvas.GetActionFrames(actionName);
            if (fixFrameIdx >= 0 && fixFrameIdx < bodyFrames.Length)
            {
                bodyFrames = new[] { bodyFrames[fixFrameIdx] };
            }
            if (bodyFrames.Length > 0)
            {
                tracks.Add(new AvatarAnimTrack(AnimSlotBody, bodyFrames));
            }

            ActionFrame[] faceFrames = canvas.GetFaceFrames(canvas.EmotionName);
            if (faceFrames.Length > 0)
            {
                tracks.Add(new AvatarAnimTrack(AnimSlotFace, new[] { faceFrames[0] }));
            }

            if (canvas.Taming != null)
            {
                canvas.LoadTamingActions();
                // sitting chair: prefer the same action as the body (usually sit) for the mount as well
                string tamingAction = canvas.TamingActions.FirstOrDefault(a => a == actionName)
                    ?? canvas.TamingActions.FirstOrDefault(a => a == "sit")
                    ?? canvas.TamingActions.FirstOrDefault(a => a == "stand1")
                    ?? canvas.TamingActions.FirstOrDefault();
                if (!string.IsNullOrEmpty(tamingAction))
                {
                    canvas.TamingActionName = tamingAction;
                    ActionFrame[] tamingFrames = canvas.GetTamingFrames(tamingAction);
                    if (tamingFrames.Length > 0)
                    {
                        tracks.Add(new AvatarAnimTrack(AnimSlotTaming, tamingFrames));
                    }
                    else
                    {
                        notes.Add("taming action empty: " + tamingAction);
                    }
                }
                else
                {
                    notes.Add("taming has no action");
                }
            }

            // chair itself (effect/effect2); read them directly even when EffectActions is empty
            AddChairEffectTrack(canvas, tracks, AvatarCanvas.IndexChairLayer1, "effect");
            AddChairEffectTrack(canvas, tracks, AvatarCanvas.IndexChairLayer2, "effect2");

            // item effects of the chair (Effect/ItemEff.img)
            AddChairEffectTrack(canvas, tracks, AvatarCanvas.IndexChairEffectLayer1, canvas.GroupChair);
            AddChairEffectTrack(canvas, tracks, AvatarCanvas.IndexChairEffectLayer2, canvas.GroupChair);

            // other part effects
            for (int i = 0; i < AvatarCanvas.PartLength; i++)
            {
                if (i == AvatarCanvas.IndexChairLayer1 || i == AvatarCanvas.IndexEffectLayer1)
                {
                    continue; // already handled above
                }

                AddPartEffectTrack(canvas, tracks, i, actionName);
            }

            if (!tracks.Any(t => t.Slot == AvatarCanvas.IndexChairLayer1))
            {
                notes.Add("chair sprite missing");
            }

            return tracks;
        }

        private static void AddChairEffectTrack(AvatarCanvas canvas, List<AvatarAnimTrack> tracks, int slot, string actionName)
        {
            if (string.IsNullOrEmpty(actionName))
            {
                return;
            }

            ActionFrame[] frames;
            try
            {
                frames = canvas.GetEffectFrames(actionName, slot);
            }
            catch
            {
                return;
            }

            if (frames != null && frames.Length > 0)
            {
                tracks.Add(new AvatarAnimTrack(slot, frames));
            }
        }

        private static void AddPartEffectTrack(AvatarCanvas canvas, List<AvatarAnimTrack> tracks, int slot, string actionName)
        {
            var layerActions = canvas.EffectActions[slot];
            if (layerActions == null || layerActions.Count <= 0)
            {
                return;
            }

            string effectAction = layerActions.FirstOrDefault(a => a == actionName)
                ?? layerActions.FirstOrDefault(a => a == "default" || a == "effect" || a == "effect2")
                ?? layerActions[0];

            AddChairEffectTrack(canvas, tracks, slot, effectAction);
        }

        private static List<GifFrame> RenderAvatarAnimFrames(AvatarCanvas canvas, List<AvatarAnimTrack> tracks, ImageHandlerConfig config, bool fixedFrameRate)
        {
            var frames = new List<GifFrame>();
            if (tracks.Count <= 0)
            {
                return frames;
            }

            int frameStep = Math.Max(1, config.MinDelay);
            int totalLength = tracks.Max(t => t.Frames.Sum(f => Math.Max(1, f.AbsoluteDelay)));
            if (totalLength <= 0)
            {
                totalLength = frameStep;
            }

            var time = new int[tracks.Count];
            var state = new int[tracks.Count];
            int[] prevIndices = null;
            int prevDelay = 0;
            int elapsed = 0;

            while (elapsed < totalLength)
            {
                int frameDelay = Math.Min(totalLength - elapsed, frameStep);
                elapsed += frameDelay;

                int[] indices = (int[])state.Clone();
                if (!fixedFrameRate && prevIndices != null && prevIndices.SequenceEqual(indices))
                {
                    prevDelay += frameDelay;
                }
                else
                {
                    if (prevIndices != null)
                    {
                        AddAvatarAnimFrame(frames, canvas, tracks, prevIndices, prevDelay);
                    }
                    prevIndices = indices;
                    prevDelay = frameDelay;
                }

                for (int i = 0; i < tracks.Count; i++)
                {
                    ActionFrame[] trackFrames = tracks[i].Frames;
                    if (trackFrames.Length <= 1)
                    {
                        continue;
                    }

                    time[i] += frameDelay;
                    int frameIndex = state[i];
                    int guard = 0;
                    int frameDelayValue = Math.Max(1, trackFrames[frameIndex].AbsoluteDelay);
                    while (time[i] >= frameDelayValue)
                    {
                        time[i] -= frameDelayValue;
                        frameIndex = (frameIndex + 1) % trackFrames.Length;
                        frameDelayValue = Math.Max(1, trackFrames[frameIndex].AbsoluteDelay);
                        if (++guard > 100000)
                        {
                            break;
                        }
                    }
                    state[i] = frameIndex;
                }
            }

            if (prevIndices != null)
            {
                AddAvatarAnimFrame(frames, canvas, tracks, prevIndices, prevDelay);
            }

            return frames;
        }

        private static void AddAvatarAnimFrame(List<GifFrame> frames, AvatarCanvas canvas, List<AvatarAnimTrack> tracks, int[] indices, int delay)
        {
            int bodyFrame = 0, faceFrame = 0, tamingFrame = 0;
            int[] effectFrames = Enumerable.Repeat(-1, AvatarCanvas.LayerSlotLength).ToArray();

            for (int i = 0; i < tracks.Count; i++)
            {
                switch (tracks[i].Slot)
                {
                    case AnimSlotBody:
                        bodyFrame = indices[i];
                        break;
                    case AnimSlotFace:
                        faceFrame = indices[i];
                        break;
                    case AnimSlotTaming:
                        tamingFrame = indices[i];
                        break;
                    default:
                        effectFrames[tracks[i].Slot] = indices[i];
                        break;
                }
            }

            var bone = canvas.CreateFrame(bodyFrame, faceFrame, tamingFrame, effectFrames);
            var frameData = canvas.DrawFrame(bone);
            if (frameData.Bitmap != null)
            {
                frames.Add(new GifFrame(frameData.Bitmap, frameData.Origin, delay));
            }
        }

        private static Brush CreateExampleBackgroundBrush(ImageHandlerConfig config)
        {
            switch (config.BackgroundType.Value)
            {
                default:
                case ImageBackgroundType.Transparent:
                    return null;

                case ImageBackgroundType.Color:
                    return new SolidBrush(config.BackgroundColor.Value);

                case ImageBackgroundType.Mosaic:
                    int blockSize = Math.Max(1, config.MosaicInfo.BlockSize);
                    var texture = new Bitmap(blockSize * 2, blockSize * 2);
                    using (var g = Graphics.FromImage(texture))
                    using (var brush0 = new SolidBrush(config.MosaicInfo.Color0))
                    using (var brush1 = new SolidBrush(config.MosaicInfo.Color1))
                    {
                        g.FillRectangle(brush0, 0, 0, blockSize, blockSize);
                        g.FillRectangle(brush0, blockSize, blockSize, blockSize, blockSize);
                        g.FillRectangle(brush1, 0, blockSize, blockSize, blockSize);
                        g.FillRectangle(brush1, blockSize, 0, blockSize, blockSize);
                    }
                    return new TextureBrush(texture);
            }
        }

        private StringLinker CreateChairStringLinker()
        {
            var stringLinker = new StringLinker();
            for (int i = 0; i < 2; i++)
            {
                Wz_File stringWz = this.StringWzNewOld[i];
                if (stringWz == null)
                {
                    stringWz = this.WzNewOld[i]?.FindNodeByPath("String").GetNodeWzFile();
                }
                if (stringWz == null)
                {
                    Wz_File sourceWz = this.WzFileNewOld[i];
                    if (sourceWz != null)
                    {
                        Wz_Node node = PluginManager.FindWz(Wz_Type.String, sourceWz);
                        if (node != null && !ReferenceEquals(node, sourceWz.Node))
                        {
                            stringWz = node.GetValueEx<Wz_File>(null);
                        }
                    }
                }
                if (stringWz == null)
                {
                    continue;
                }

                stringLinker.Load(stringWz,
                    this.ItemWzNewOld[i] ?? this.WzNewOld[i]?.FindNodeByPath("Item").GetNodeWzFile(),
                    this.EtcWzNewOld[i] ?? this.WzNewOld[i]?.FindNodeByPath("Etc").GetNodeWzFile(),
                    this.QuestWzNewOld[i] ?? this.WzNewOld[i]?.FindNodeByPath("Quest").GetNodeWzFile());
                break;
            }
            return stringLinker;
        }

        private static string GetChairName(StringLinker stringLinker, int chairID)
        {
            StringResult sr;
            if (stringLinker != null && stringLinker.StringItem.TryGetValue(chairID, out sr) && !string.IsNullOrEmpty(sr.Name))
            {
                return sr.Name;
            }
            return "未知的道具";
        }

        /// <summary>
        /// Finds the WZ root node of the given type (Item, Character, ...).
        /// <br/>PluginManager.FindWz returns the root of the source WZ file when the lookup fails, so the result is verified here.
        /// </summary>
        private Wz_Node FindWzRoot(string typeName)
        {
            foreach (var wzFile in new[] { this.WzFileNewOld[0], this.WzFileNewOld[1] })
            {
                if (wzFile == null)
                {
                    continue;
                }

                if (string.Equals(wzFile.Type.ToString(), typeName, StringComparison.OrdinalIgnoreCase))
                {
                    return wzFile.Node;
                }

                Wz_Node node = PluginManager.FindWz(typeName, wzFile);
                if (node != null && !ReferenceEquals(node, wzFile.Node))
                {
                    return node;
                }
            }

            return PluginManager.FindWz(typeName);
        }

        /// <summary>
        /// Resolves a path such as "Item\Install\03019.img\03019829" only when it actually exists.
        /// </summary>
        private Wz_Node ResolveWzNode(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                return null;
            }

            string[] parts = fullPath.Split('\\', '/');
            Wz_Node node = this.FindWzRoot(parts[0]);
            for (int i = 1; i < parts.Length; i++)
            {
                if (node == null)
                {
                    return null;
                }

                node = node.Nodes[parts[i]];
                if (node == null)
                {
                    return null;
                }

                Wz_Node imgNode = ExtractImageNode(node);
                if (imgNode != null)
                {
                    node = imgNode;
                }
            }

            return node;
        }

        private Wz_Node FindChairItemNode(int chairID)
        {
            string idName = chairID.ToString("D8");
            var paths = new List<string>
            {
                // same resolution order as the current ItemTooltip
                string.Format(@"Item\Install\0{0:D}.img\{1}", chairID / 1000, idName),
                string.Format(@"Item\Install\0{0:D}.img\{1}", chairID / 100, idName),
                string.Format(@"Item\Install\{0:D4}.img\{1}", chairID / 10000, idName),
                string.Format(@"Item\Install\0{0:D}.img\{1}", chairID / 10000, idName),
            };

            foreach (string path in paths)
            {
                Wz_Node node = this.ResolveWzNode(path);
                if (IsItemNode(node))
                {
                    return node;
                }
            }

            // some WZ files do not zero pad the node name to 8 digits, so match on the numeric id
            return this.FindItemNodeByID(chairID, "Install", "Cash", "Consume", "Etc");
        }

        private static bool IsItemNode(Wz_Node node)
        {
            return node != null && node.FindNodeByPath("info") != null;
        }

        private Wz_Node FindItemNodeByID(int id, params string[] categoryNames)
        {
            foreach (string categoryName in categoryNames)
            {
                var map = this.GetItemNodeMap(categoryName);
                Wz_Node node;
                if (map.TryGetValue(id, out node) && IsItemNode(node))
                {
                    return node;
                }
            }
            return null;
        }

        /// <summary>
        /// Scans the images under Item\&lt;category&gt; and builds a lookup table keyed by id (cached per category).
        /// </summary>
        private Dictionary<int, Wz_Node> GetItemNodeMap(string categoryName)
        {
            Dictionary<int, Wz_Node> map;
            if (this.itemNodeCache.TryGetValue(categoryName, out map))
            {
                return map;
            }

            map = new Dictionary<int, Wz_Node>();
            this.itemNodeCache[categoryName] = map;

            Wz_Node categoryRoot = this.ResolveWzNode("Item\\" + categoryName);
            var imgNodes = categoryRoot == null ? null : categoryRoot.Nodes;
            if (imgNodes == null)
            {
                return map;
            }

            foreach (var imgNode in imgNodes)
            {
                Wz_Node imgRoot = ExtractImageNode(imgNode);
                if (imgRoot == null)
                {
                    continue;
                }

                foreach (var child in imgRoot.Nodes)
                {
                    string childText = child.Text;
                    if (childText.EndsWith(".img", StringComparison.OrdinalIgnoreCase))
                    {
                        childText = childText.Substring(0, childText.Length - 4);
                    }

                    int id;
                    if (!int.TryParse(childText, out id) || map.ContainsKey(id))
                    {
                        continue;
                    }

                    // expand the node when it is stored as a sub image
                    map[id] = ExtractImageNode(child) ?? child;
                }
            }

            return map;
        }

        /// <summary>
        /// Loads an avatar code (same format as AvatarForm.LoadCode) and adds its parts to the canvas.
        /// </summary>
        /// <returns>Number of parts that were added.</returns>
        private int LoadAvatarCodeIntoCanvas(AvatarCanvas canvas, string code, List<string> notes)
        {
            if (string.IsNullOrEmpty(code))
            {
                notes.Add("avatar code empty");
                return 0;
            }

            var matches = Regex.Matches(code,
                @"(?:"
                    + @"s?(?<id>\d+)"
                    + @"(?<mix>\+(?<mixColor>[0-8])\*(?<mixRate>\d{1,2}))?"
                    + @"(?<prism>(?:\+\d+h\d+s\d+v\d+(?:PB)?){0,2})"
                    + "|"
                    + @"(?<co>co(?<coID>\d+)x(?<coX>-?\d+)y(?<coY>-?\d+))"
                + @")(?=[,\s]|$)");

            int loaded = 0;
            var missing = new List<string>();

            foreach (Match m in matches)
            {
                if (m.Groups["co"].Success)
                {
                    canvas.CustomOrigin[m.Groups["coID"].Value] = new Point(
                        int.Parse(m.Groups["coX"].Value),
                        int.Parse(m.Groups["coY"].Value));
                    continue;
                }

                int id;
                if (!m.Groups["id"].Success || !int.TryParse(m.Groups["id"].Value, out id))
                {
                    continue;
                }

                AvatarPart part = this.AddAvatarCodePart(canvas, id);
                if (part == null)
                {
                    missing.Add(id.ToString());
                    continue;
                }

                loaded++;
                ApplyAvatarCodeMixAndPrism(part, m);
            }

            if (missing.Count > 0)
            {
                notes.Add("avatar part not found: " + string.Join("/", missing));
            }

            if (canvas.Longcoat != null)
            {
                if (canvas.Pants != null)
                {
                    canvas.Pants.Visible = false;
                }
                if (canvas.Coat != null)
                {
                    canvas.Coat.Visible = false;
                }
                canvas.Longcoat.Visible = true;
            }

            return loaded;
        }

        private AvatarPart AddAvatarCodePart(AvatarCanvas canvas, int id)
        {
            Wz_Node imgNode = this.FindGearNode(id);
            if (imgNode != null)
            {
                GearType gearType = Gear.GetGearType(id);
                if (gearType == GearType.taming
                    || gearType == GearType.taming2
                    || gearType == GearType.taming3
                    || gearType == GearType.tamingChair)
                {
                    return canvas.AddTamingPart(imgNode,
                        BitmapOrigin.CreateFromNode(imgNode.FindNodeByPath("info\\icon"), PluginManager.FindWz),
                        id,
                        false);
                }

                AvatarPart part = canvas.AddPart(imgNode);
                if (part == null)
                {
                    return null;
                }

                if (part == canvas.Body) // sync the head with the body type
                {
                    Wz_Node headNode = this.FindGearNode(10000 + id % 10000);
                    if (headNode != null)
                    {
                        canvas.AddPart(headNode);
                    }
                }
                else if (part == canvas.Head)
                {
                    Wz_Node bodyNode = this.FindGearNode(id % 10000);
                    if (bodyNode != null)
                    {
                        canvas.AddPart(bodyNode);
                    }
                }
                return part;
            }

            // riding skill
            Wz_Node skillNode = this.FindSkillNode(id);
            if (skillNode != null)
            {
                int tamingMobID = skillNode.Nodes["vehicleID"].GetValueEx<int>(0);
                if (tamingMobID == 0)
                {
                    tamingMobID = this.ResolveWzNode(string.Format(@"Skill\RidingSkillInfo.img\{0:D7}\vehicleID", id)).GetValueEx<int>(0);
                }
                if (tamingMobID != 0)
                {
                    Wz_Node tamingMobNode = this.ResolveWzNode(string.Format(@"Character\TamingMob\{0:D8}.img", tamingMobID));
                    if (tamingMobNode != null)
                    {
                        return canvas.AddTamingPart(tamingMobNode,
                            BitmapOrigin.CreateFromNode(skillNode.Nodes["icon"], PluginManager.FindWz),
                            id,
                            true);
                    }
                }
                return null;
            }

            Wz_Node itemNode = this.FindItemNode(id);
            if (itemNode != null)
            {
                if (id / 10000 == 501) // effect item
                {
                    return canvas.AddEffectPart(itemNode);
                }

                int tamingMobID = itemNode.FindNodeByPath("info\\tamingMob").GetValueEx<int>(0);
                if (tamingMobID == 0)
                {
                    tamingMobID = itemNode.FindNodeByPath("info\\customChair\\self\\tamingMob").GetValueEx<int>(0);
                }
                if (tamingMobID != 0)
                {
                    Wz_Node tamingMobNode = this.ResolveWzNode(string.Format(@"Character\TamingMob\{0:D8}.img", tamingMobID));
                    if (tamingMobNode != null)
                    {
                        canvas.RemoveChairPart();
                        return canvas.AddTamingPart(tamingMobNode,
                            BitmapOrigin.CreateFromNode(tamingMobNode.FindNodeByPath("info\\icon"), PluginManager.FindWz),
                            tamingMobID,
                            false,
                            itemNode.FindNodeByPath("info\\group\\sit\\0\\bodyRelMove").GetValueEx<Wz_Vector>(null));
                    }
                }

                Wz_Vector brm = itemNode.FindNodeByPath("info\\bodyRelMove").GetValueEx<Wz_Vector>(null);
                bool isSitActionExists = itemNode.FindNodeByPath("info\\sitAction").GetValueEx<string>(null) != null;
                if (id / 10000 == 301 || id / 1000 == 5204 || brm != null || isSitActionExists)
                {
                    canvas.RemoveTamingPart();
                    return canvas.AddChairPart(itemNode,
                        BitmapOrigin.CreateFromNode(itemNode.FindNodeByPath("info\\icon"), PluginManager.FindWz),
                        id,
                        brm,
                        brm != null && isSitActionExists);
                }
            }

            return null;
        }

        private static void ApplyAvatarCodeMixAndPrism(AvatarPart part, Match m)
        {
            int mixColor, mixOpacity;
            if (m.Groups["mixColor"].Success && m.Groups["mixRate"].Success
                && int.TryParse(m.Groups["mixColor"].Value, out mixColor)
                && int.TryParse(m.Groups["mixRate"].Value, out mixOpacity))
            {
                part.MixColor = mixColor;
                part.MixOpacity = mixOpacity;
            }

            if (m.Groups["prism"].Success)
            {
                ApplyAvatarCodePrism(part, m.Groups["prism"].Value);
            }
        }

        private static void ApplyAvatarCodePrism(AvatarPart part, string prismStr)
        {
            var prismMatches = Regex.Matches(prismStr, @"\+(\d+)h(\d+)s(\d+)v(\d+)(PB)?");
            if (prismMatches.Count >= 2)
            {
                GearType partType = Gear.GetGearType(part.ID.Value);
                if (Gear.IsWeapon(partType) || Gear.IsCashWeapon(partType))
                {
                    ApplyAvatarCodePrismData(part, prismMatches[1], PrismDataCollection.PrismDataType.WeaponEffect);
                }
            }
            if (prismMatches.Count >= 1)
            {
                ApplyAvatarCodePrismData(part, prismMatches[0], PrismDataCollection.PrismDataType.Default);
            }
        }

        private static void ApplyAvatarCodePrismData(AvatarPart part, Match prismMatch, PrismDataCollection.PrismDataType dataType)
        {
            int type, hue, saturation, brightness;
            if (int.TryParse(prismMatch.Groups[1].Value, out type)
                && int.TryParse(prismMatch.Groups[2].Value, out hue)
                && int.TryParse(prismMatch.Groups[3].Value, out saturation)
                && int.TryParse(prismMatch.Groups[4].Value, out brightness))
            {
                part.PrismData.Set(dataType, type, hue, saturation, brightness, prismMatch.Groups[5].Success);
            }
        }

        private Wz_Node FindGearNode(int id)
        {
            string imgName = id.ToString("D8") + ".img";
            foreach (string path in new[] { $@"Character\{imgName}", $@"Character\Hair\{imgName}", $@"Character\Face\{imgName}" })
            {
                Wz_Node node = this.ResolveWzNode(path);
                if (node != null && node.FindNodeByPath("info") != null)
                {
                    return node;
                }
            }

            // brute force the remaining sub folders (Character\Weapon etc.)
            Wz_Node characWz = this.ResolveWzNode("Character");
            if (characWz == null)
            {
                return null;
            }

            foreach (var node1 in characWz.Nodes)
            {
                if (node1.Text.Contains("_Canvas"))
                {
                    continue;
                }

                Wz_Node imgNode = node1.Text == imgName ? node1 : node1.Nodes[imgName];
                if (imgNode == null)
                {
                    continue;
                }

                Wz_Node extracted = ExtractImageNode(imgNode);
                if (extracted != null && extracted.FindNodeByPath("info") != null)
                {
                    return extracted;
                }
            }

            return null;
        }

        private static Wz_Node ExtractImageNode(Wz_Node node)
        {
            if (node == null)
            {
                return null;
            }

            Wz_Image img = node.GetValue<Wz_Image>();
            if (img != null && img.TryExtract())
            {
                return img.Node;
            }
            return null;
        }

        private Wz_Node FindSkillNode(int id)
        {
            Wz_Node skillWz = this.ResolveWzNode("Skill");
            if (skillWz == null)
            {
                return null;
            }

            string idName = id.ToString();
            foreach (var node1 in skillWz.Nodes)
            {
                if (!idName.StartsWith(node1.Text.Replace(".img", "")))
                {
                    continue;
                }

                Wz_Node imgNode = ExtractImageNode(node1);
                Wz_Node skillNode = imgNode?.FindNodeByPath(idName);
                if (skillNode != null && skillNode.Nodes.Count > 0)
                {
                    return skillNode;
                }
            }

            return null;
        }

        private Wz_Node FindItemNode(int id)
        {
            string idName = id.ToString("D8");
            string categoryName;
            switch (idName.Substring(0, 2))
            {
                case "02": categoryName = "Consume"; break;
                case "03": categoryName = "Install"; break;
                case "04": categoryName = "Etc"; break;
                case "05": categoryName = "Cash"; break;
                case "50": categoryName = "Pet"; break;
                default: categoryName = "Install"; break;
            }

            Wz_Node categoryRoot = this.ResolveWzNode("Item\\" + categoryName);
            if (categoryRoot != null)
            {
                Wz_Node found = FindItemNodeInCategory(categoryRoot, idName);
                if (found != null)
                {
                    return found;
                }
            }

            // scan every category in case the category name differs
            Wz_Node itemWz = this.ResolveWzNode("Item");
            if (itemWz != null)
            {
                foreach (var categoryNode in itemWz.Nodes)
                {
                    Wz_Node found = FindItemNodeInCategory(categoryNode, idName);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }

        private static Wz_Node FindItemNodeInCategory(Wz_Node categoryNode, string idName)
        {
            foreach (var imgNode in categoryNode.Nodes)
            {
                string imgText = imgNode.Text.Replace(".img", "");
                if (imgText.Length > 0 && !idName.StartsWith(imgText))
                {
                    continue;
                }

                Wz_Node imgRoot = ExtractImageNode(imgNode);
                Wz_Node itemNode = imgRoot?.Nodes[idName];
                if (itemNode != null && IsItemNode(itemNode))
                {
                    return itemNode;
                }
            }

            return null;
        }

        private sealed class AvatarAnimTrack
        {
            public AvatarAnimTrack(int slot, ActionFrame[] frames)
            {
                this.Slot = slot;
                this.Frames = frames;
            }

            public int Slot { get; private set; }
            public ActionFrame[] Frames { get; private set; }
        }
    }
}
