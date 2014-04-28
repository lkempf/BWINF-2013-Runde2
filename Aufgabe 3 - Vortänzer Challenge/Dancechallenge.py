 #
 # Folgende Spielobjekte sind definiert:
 #
 # zustand.DanceRobot
 #      
 #
 # Eigenschaften des DanceRobot-Spielobjekts:
 # DanceRobot.letzterTanz()
 # DanceRobot.identifikation()
 # DanceRobot.istVortaenzer()
 # DanceRobot.strafpunkte()
 #
 # Zustandsmanipulation mit dem DanceRobot-Spielobjekt:
 # zustand.listeDanceRobot()
 #
 #
 # Sie koennen folgende Aktionen ausfuehren:
 #
 # zug.ausgabe(text) - Damit kannst du eine Debugausgabe machen. Verwende nicht print.
 # 
 # zug.tanzen(tanz)
 #     
 #
 #

# Hier implementiert ihr den Zug, den eure KI machen soll.
#
# id       eine eindeutige Zahl, die eure KI identifiziert
# zustand  schau in den oberen Kommentarblock
# zug      schau in den oberen Kommentarblock

import re
import math
import random

def zug(id, zustand, zug):
 zug.ausgabe("Neuer Zug")
 zug.ausgabe("Meine Strafpunkte:")
 zug.ausgabe(getMe(id, zustand).strafpunkte())
 if ichBinVortaenzer(id, zustand):
   vor(id, zustand, zug)
 else:
   nach(id, zustand, zug, tanzZumNachtanzen(id, zustand))

def vor(id, zustand, zug):
 zug.ausgabe("Ich bin Vortaenzer")
 zug.tanzen(createDance())
 pass

def nach(id, zustand, zug, tanz):
 zug.ausgabe("Ich bin Nachtaenzer")
 zug.ausgabe(tanz)
 zug.tanzen(imitateDance(tanz, zug))
 pass

def ichBinVortaenzer(id, zustand):
 return getMe(id, zustand).istVortaenzer()

def tanzZumNachtanzen(id, zustand):
 for i in range(0, len(zustand.listeDanceRobot())):
   if zustand.listeDanceRobot()[i].istVortaenzer():
     return zustand.listeDanceRobot()[i].letzterTanz()

def getMe(id, zustand):
 for i in range(0, len(zustand.listeDanceRobot())):
   if zustand.listeDanceRobot()[i].identifikation() == id:
     return zustand.listeDanceRobot()[i]

def createDance():
    programPool = []
    programPool.append("464F-.B-..")
    programPool.append("Fl9FFr9B..")
    programPool.append("566F..2l..")
    programPool.append("499B..ll-.")
    programPool.append("999l3B....")
    programPool.append("999lBrB...")
    programPool.append("99B-.9-F..")
    programPool.append("789rFlF...")

    return random.choice(programPool)

def checkDance(dance, newDance):
    danceProgram = DanceProgram(newDance)
    if danceProgram.IsValid():
        simulation = DanceSimulation()
        simulation.SetOriginalProgram(dance)
        simulation.SetImitatingProgram(newDance)
        if simulation.GetPenaltyPoints() == 0:
            return True
    return False

def imitateDance(dance, zug):
    robot = DanceRobot(dance)
    zug.ausgabe("Imitieren startet")
    regex = re.compile(r"(?!\.)(.+?)\1+(?<=\D)")
    greedyRegex = re.compile(r"(?!\.)(.+)\1+(?<=\D)")
    currentBestDance = ""
    newDance = ""

    newDance = tryCompressWithAutoComplete(dance, zug, regex)
    zug.ausgabe(newDance)
    if newDance != "" and checkDance(dance, newDance):
        currentBestDance = newDance

    newDance = tryCompress(dance, zug, regex)
    zug.ausgabe(newDance)
    if checkDance(dance, newDance) and (len(newDance) < len(currentBestDance) or currentBestDance == ""):
        currentBestDance = newDance
    
    newDance = tryCompressWithAutoComplete(dance, zug, greedyRegex)
    zug.ausgabe(newDance)
    if len(newDance) > 0 and len(currentBestDance) > len(newDance): #Gueltigkeit pruefen, ist bei greedyRegex nicht garantiert
        if checkDance(dance, newDance):
            currentBestDance = newDance

    newDance = tryCompress(dance, zug, greedyRegex)
    zug.ausgabe(newDance)
    if len(currentBestDance) > len(newDance): #Gueltigkeit pruefen, ist bei greedyRegex nicht garantiert
        if checkDance(dance, newDance):
            currentBestDance = newDance
    
    newDance = tryCompressWithGreedyAutoComplete(dance, zug, regex, greedyRegex)
    zug.ausgabe(newDance)
    if len(newDance) > 0 and len(currentBestDance) > len(newDance): #Gueltigkeit pruefen, ist bei greedyRegex nicht garantiert
        if checkDance(dance, newDance):
            currentBestDance = newDance
    
    zug.ausgabe(currentBestDance)
    return currentBestDance

def getRepeatedProgramString(orderChar, iterations):
    if iterations > 1: #Eine Wiederholung macht keinen Sinn
        if iterations < 10:
            return str(iterations) + orderChar + "."
        else:
            returnString = ""
            newSection = ""
            while(True):
                oldIterations = iterations
                for i in reversed(range(2,10)):
                    if iterations < 10: #Nichts mehr zu tun
                        break
                    if iterations % i == 0:
                        if newSection == "":
                            newSection = orderChar
                        newSection = str(i) + newSection + "."
                        iterations = iterations / i
                        break

                if iterations == oldIterations:
                    if iterations < 10:
                        return returnString + str(iterations) + newSection + "."
                    else:
                        returnString += newSection
                        returnString += "9" + orderChar + "."
                        iterations = iterations - 9
    else:
        return orderChar

def createLoop(match):
    return getRepeatedProgramString(match.group(1), len(match.group()) / len(match.group(1)))

def tryCompress(dance, zug, regex):
    zug.ausgabe("Schleifenfindung gestartet")
    oldDance = dance + "."
    while(len(oldDance) > len(dance)):
        oldDance = dance
        dance = regex.sub(createLoop, dance)
    zug.ausgabe("Komprimiertes Ergebnis")
    zug.ausgabe(dance)
    badLoopRegex = re.compile("([2][FB-lr][\.])")
    dance = badLoopRegex.sub(replaceBadLoop, dance)
    return dance

def replaceBadLoop(match):
    return match.group()[1] + match.group()[1]

def tryCompressWithAutoComplete(dance, zug, regex):
    return tryCompressWithGreedyAutoComplete(dance, zug, regex, regex)

def tryCompressWithGreedyAutoComplete(dance, zug, regex, greedyRegex):
    zug.ausgabe("AutoComplete gestartet")
    if len(dance) != 255:
        return ""

    match = greedyRegex.match(dance)

    if match:
        if len(match.group()) == 255 and len(match.group(1)) == 1:
            return "994" + match.group(1) + "..."
        zug.ausgabe("Kein Match")
        endString = dance[len(match.group()) - 255:]
        zug.ausgabe(endString)
        if match.group(1).startswith(endString):
            zug.ausgabe(dance[len(endString):len(match.group(1))])
            dance += dance[len(endString):len(match.group(1))]
            dance = getRepeatedProgramString(match.group(1), len(dance) / len(match.group(1)))
            zug.ausgabe(dance)
            return tryCompress(dance, zug, regex)
    return ""