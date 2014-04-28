"""
Instruction:
0 = Forwards
1 = Backwards
2 = left
3 = right
4 = pause
5 = finished
"""

class DanceProgram(object):
    instructionSequence = ""
    instructionPointer = 0

    loopIterationCounters = []
    loopLimits = []
    loopJumpBackIndices = []

    def __init__(self, instructionSequence):
        self.instructionSequence = instructionSequence

    def NextInstruction(self):
        finished = False
        while not finished:
            finished = True
            if self.instructionPointer >= len(self.instructionSequence):
                return 5
            instructionChar = self.instructionSequence[self.instructionPointer]
            self.instructionPointer += 1

            if instructionChar == 'F':
                return 0
            elif instructionChar == 'B':
                return 1
            elif instructionChar == 'l':
                return 2
            elif instructionChar == 'r':
                return 3
            elif instructionChar == '-':
                return 4
            elif instructionChar == '.':
                if self.loopIterationCounters[-1] >= self.loopLimits[-1] - 1:
                    self.loopIterationCounters.pop()
                    self.loopLimits.pop()
                    self.loopJumpBackIndices.pop()
                else:
                    self.loopIterationCounters.append(self.loopIterationCounters.pop() + 1)
                    self.instructionPointer = self.loopJumpBackIndices[-1]
                finished = False
            else:
                if instructionChar.isdigit():
                    loopLimit = int(instructionChar)
                    self.loopLimits.append(loopLimit)
                    self.loopIterationCounters.append(0)
                    self.loopJumpBackIndices.append(self.instructionPointer)
                    finished = False
                else:
                    raise ValueError(instructionChar + " is no valid instruction char. Please check the input string.")

    def IsValid(self):
        loopCounter = 0
        for c in self.instructionSequence:
            if c == 'F' or c == 'B' or c == 'l' or c == 'r' or c == '-':
                continue
            elif c == '.':
                loopCounter -= 1
                if loopCounter < 0:
                    return False
            elif c.isdigit():
                loopCounter += 1
            else: return False
        return loopCounter == 0